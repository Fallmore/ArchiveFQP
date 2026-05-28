using ArchiveFqp.Interfaces.FileUpload;
using ArchiveFqp.Interfaces.Hash;
using ArchiveFqp.Models.FileUpload;
using ArchiveFqp.Models.Hash;
using ArchiveFqp.Models.Settings;
using ArchiveFqp.Models.Settings.SettingsArchive;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Components.Forms;
using System.Text.RegularExpressions;
using UglyToad.PdfPig.Content;

namespace ArchiveFqp.Services.FileUpload
{
    /// <summary>
    /// Сервис для загрузки файлов учебных работ с поддержкой динамических типов
    /// </summary>
    public class WorkFileUploadService : BaseFileUploadService
    {
        private readonly IHashService _hashService;
        private SettingsArchive _settings;
        private Dictionary<string, FileTypeConfig> _fileTypeConfigs = null!;

        public override event EventHandler<FileUploadProgressEventArgs>? ProgressChanged;

        protected virtual void OnProgressChanged(FileUploadProgressEventArgs e)
        {
            ProgressChanged?.Invoke(this, e);
        }

        public WorkFileUploadService(IWebHostEnvironment environment, ILogger<WorkFileUploadService> logger,
            IHashService hashService, SettingsArchive settings)
            : base(environment, logger)
        {
            _hashService = hashService;
            _settings = settings;
            _settings.SettingsChanged += SettingsChanged;

            if (string.IsNullOrEmpty(_settings.FilesRootPath))
            {
                _settings.FilesRootPath = environment.ContentRootPath;
                _settings.SaveSettings();
            }
        }

        private void SettingsChanged(object? sender, SettingsArchive e)
        {
            _settings = e;
        }

        /// <summary>
        /// Инициализация конфигураций типов файлов из настроек
        /// </summary>
        public void InitializeFileTypeConfigs(BaseSettings settingsFiles, string workType)
        {
            _fileTypeConfigs = new Dictionary<string, FileTypeConfig>();

            _settings.RequiredFiles.TryGetValue(workType, out var requiredFiles);

            foreach (var extensionMapping in _settings.AllowedFiles)
            {
                var config = new FileTypeConfig
                {
                    Key = extensionMapping.Key,
                    DisplayName = extensionMapping.Key,
                    AllowedExtensions = extensionMapping.Value,
                    // Определяем префикс и другие свойства на основе имени
                    FilePrefix = GenerateFilePrefix(extensionMapping.Key),
                    IsRequired = requiredFiles == null ? false : requiredFiles.Contains(extensionMapping.Key)
                };

                _fileTypeConfigs[config.Key] = config;
            }
        }

        /// <summary>
        /// Генерация ключа из отображаемого имени
        /// </summary>
        private string GenerateKey(string displayName)
        {
            // Удаляем пробелы и специальные символы, транслитерируем
            var normalized = Regex.Replace(displayName, @"[^\w\s]", "");
            normalized = Regex.Replace(normalized, @"\s+", "");
            return normalized;
        }

        /// <summary>
        /// Генерация префикса для файла
        /// </summary>
        private string GenerateFilePrefix(string displayName)
        {
            return displayName switch
            {
                var name when name.Contains("Word", StringComparison.OrdinalIgnoreCase) => "explanation_word",
                var name when name.Contains("Pdf", StringComparison.OrdinalIgnoreCase) => "explanation_pdf",
                var name when name.Contains("Презентация", StringComparison.OrdinalIgnoreCase) => "presentation",
                var name when name.Contains("Исходный код", StringComparison.OrdinalIgnoreCase) => "source",
                var name when name.Contains("База данных", StringComparison.OrdinalIgnoreCase) => "database",
                var name when name.Contains("Пароли", StringComparison.OrdinalIgnoreCase) => "passwords",
                _ => GenerateKey(displayName).ToLower()
            };
        }

        public override async Task<FileUploadResult> UploadFileAsync(IFileUploadContext context, CancellationToken cancellationToken = default)
        {
            var result = await UploadFilesWithHashAsync(context, cancellationToken);
            return result.FileResults.FirstOrDefault() ?? new FileUploadResult { Success = false };
        }

        public override async Task<IEnumerable<FileUploadResult>> UploadFilesAsync(IFileUploadContext context, CancellationToken cancellationToken = default)
        {
            var result = await UploadFilesWithHashAsync(context, cancellationToken);
            return result.FileResults;
        }

        public override async Task<FileUploadWithHashResult> UploadFileWithHashAsync(IFileUploadContext context, CancellationToken cancellationToken = default)
        {
            var result = await UploadFilesWithHashAsync(context, cancellationToken);
            return result;
        }

        public override async Task<FileUploadWithHashResult> UploadFilesWithHashAsync(IFileUploadContext context, CancellationToken cancellationToken = default)
        {
            if (context is not WorkUploadContext workContext)
            {
                throw new ArgumentException("Контекст должен быть типа WorkUploadContext", nameof(context));
            }

            FileUploadWithHashResult result = new();
            List<IBrowserFile> filesToHash = new();
            List<Task<FileUploadResult>> uploadTasks = new();

            try
            {
                // Строим путь к папке
                string relativeFolderPath = BuildFolderPath(context);
#warning Раскоментировать после всех проверок
                if (!workContext.IsTemp || DirectoryExists(relativeFolderPath))
                {
                    result.FileResults = [new FileUploadResult{
                        Success = false,
                        ErrorMessage = "Работа данного студента уже в архиве!"}];
                    return result;
                }
                if (workContext.IsTemp) relativeFolderPath = Path.Combine(relativeFolderPath, _settings.FolderTempName);
                string fullFolderPath = EnsureDirectoryExists(relativeFolderPath);

                // Динамическая загрузка всех файлов из контекста
                foreach (var fileMapping in workContext.Files)
                {
                    string fileTypeKey = fileMapping.Key;
                    var files = fileMapping.Value;

                    if (!_fileTypeConfigs.TryGetValue(fileTypeKey, out var config))
                    {
                        _logger.LogWarning("Неизвестный тип файла: {FileType}", fileTypeKey);
                        continue;
                    }

                    foreach (var file in files)
                    {
                        filesToHash.Add(file);
                        uploadTasks.Add(UploadSpecificFile(
                            file,
                            fullFolderPath,
                            relativeFolderPath,
                            fileTypeKey,
                            config,
                            cancellationToken));
                    }
                }

                // Ждем завершения всех загрузок
                FileUploadResult[] uploadResults = await Task.WhenAll(uploadTasks);
                result.FileResults.AddRange(uploadResults);

                if (cancellationToken.IsCancellationRequested || result.FailedCount != 0)
                {
                    // Удаляем файлы, которые успели загрузиться, и последнюю папку в пути
                    DirectoryDelete(relativeFolderPath);
                    return result;
                }

                if (filesToHash.Count != 0)
                {
                    result.HashesInfo = await _hashService.GetFileHashesInfoAsync(filesToHash, cancellationToken);

                    _logger.LogDebug("Вычислен составной хеш {Hash} для {Count} файлов",
                        result.HashesInfo.CompositeHash, result.HashesInfo.FileCount);
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка загрузки файлов для работы {WorkTitle}", context.WorkTitle);

                result.FileResults.Add(new FileUploadResult
                {
                    Success = false,
                    ErrorMessage = ex.Message
                });
            }

            return result;
        }

        /// <summary>
        /// Загружает файл на сервер с динамической конфигурацией
        /// </summary>
        private async Task<FileUploadResult> UploadSpecificFile(
            IBrowserFile file, string fullFolderPath, string relativeFolderPath,
            string fileTypeKey, FileTypeConfig config, CancellationToken cancellationToken)
        {
            FileUploadResult result = new()
            {
                OriginalFileName = file.Name,
                FileTypeName = config.DisplayName,
                UploadedAt = DateTime.UtcNow
            };

            string filePath = "";

            try
            {
                // Валидация файла
                if (!ValidateFile(file, config, out string? errorMessage))
                {
                    result.Success = false;
                    result.ErrorMessage = errorMessage;
                    return result;
                }

                // Генерация имени файла
                string extension = Path.GetExtension(file.Name);
                string storedFileName = $"{config.DisplayName}{extension}";

                // Если файл с таким именем уже существует - добавляем номер
                // Если нужно - расскоментируйте
                //storedFileName = GetUniqueFileName(fullFolderPath, storedFileName);

                filePath = Path.Combine(fullFolderPath, storedFileName);

                // Сохранение файла
                using Stream stream = file.OpenReadStream(_settings.MaxFileSize, cancellationToken);
                using FileStream fileStream = new(filePath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite, 4096, true);

                byte[] buffer = new byte[1024 * 10];
                int bytesRead = 0;
                long totalBytesRead = 0;
                long fileSize = file.Size;

                while ((bytesRead = await stream.ReadAsync(buffer, cancellationToken)) != 0)
                {
                    await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken);
                    totalBytesRead += bytesRead;

                    // Уведомляем подписчиков о прогрессе
                    OnProgressChanged(new FileUploadProgressEventArgs
                    {
                        FileName = file.Name,
                        ProgressPercent = (decimal)totalBytesRead / fileSize,
                        BytesUploaded = totalBytesRead,
                        TotalBytes = fileSize,
                        FileType = config.DisplayName
                    });
                }

                result.Success = true;
                result.StoredFileName = storedFileName;
                result.RelativePath = Path.Combine(relativeFolderPath, storedFileName).Replace('\\', '/');
                result.FullPath = filePath;
                result.FileSize = totalBytesRead;
                result.ContentType = file.ContentType;

                _logger.LogDebug("Загрузка файла {FileName} в {Path}", storedFileName, filePath);
            }
            catch (OperationCanceledException)
            {
                _logger.LogDebug("Загрузка файла {FileName} была отменена", file.Name);
                result.Success = false;
                result.ErrorMessage = "Загрузка была отменена";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка загрузки файла {FileName}", file.Name);
                result.Success = false;
                result.ErrorMessage = ex.Message;
            }

            return result;
        }

        /// <summary>
        /// Валидация файла с динамической конфигурацией
        /// </summary>
        public bool ValidateFile(IBrowserFile file, FileTypeConfig config, out string? errorMessage)
        {
            errorMessage = null;

            // Проверка размера
            if (file.Size > _settings.MaxFileSize)
            {
                errorMessage = $"Файл слишком большой. Максимальный размер: {_settings.MaxFileSize / 1024 / 1024} MB";
                return false;
            }

            // Проверка расширения
            string extension = Path.GetExtension(file.Name).ToLowerInvariant();

            if (!config.AllowedExtensions.Contains(extension))
            {
                errorMessage = $"Для {config.DisplayName} разрешены только {string.Join(", ", config.AllowedExtensions)} файлы";
                return false;
            }

            return true;
        }

        public override bool ValidateFile(IBrowserFile file, FileType fileType, out string? errorMessage)
        {
            // Для обратной совместимости со старым кодом
            // Находим конфигурацию по старому типу
            var config = _fileTypeConfigs.Values.FirstOrDefault(c =>
                c.DisplayName.Contains(GetDisplayNameFromOldFileType(fileType)));

            if (config != null)
            {
                return ValidateFile(file, config, out errorMessage);
            }

            errorMessage = "Неизвестный тип файла";
            return false;
        }

        private string GetDisplayNameFromOldFileType(FileType fileType)
        {
            return fileType switch
            {
                var ft when ft == FileType.ExplanatoryNoteWord => "Пояснительная записка Word",
                var ft when ft == FileType.ExplanatoryNotePdf => "Пояснительная записка Pdf",
                var ft when ft == FileType.Presentation => "Презентация",
                var ft when ft == FileType.SourceCode => "Исходный код",
                var ft when ft == FileType.DatabaseBackup => "База данных",
                var ft when ft == FileType.PasswordFile => "Пароли",
                _ => string.Empty
            };
        }

        /// <summary>
        /// Получить уникальное имя файла, если файл с таким именем уже существует
        /// </summary>
        private string GetUniqueFileName(string directory, string fileName)
        {
            string nameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
            string extension = Path.GetExtension(fileName);
            string fullPath = Path.Combine(directory, fileName);
            int counter = 1;

            while (File.Exists(fullPath))
            {
                string newFileName = $"{nameWithoutExt}_{counter}{extension}";
                fullPath = Path.Combine(directory, newFileName);
                counter++;
            }

            return Path.GetFileName(fullPath);
        }

        public override async Task<HashVerificationResult> VerifyUploadedFilesAsync(string sourceHash, string relativeFolderPath, CancellationToken cancellationToken = default)
        {
            HashVerificationResult result = new();
            result.IsValid = false;

            try
            {
                string fullFolderPath = Path.Combine(_settings.FilesRootPath, _settings.FolderDataName, _settings.FolderWorksName, relativeFolderPath);

                if (!Directory.Exists(fullFolderPath))
                {
                    result.Message = "Папка не найдена";
                    return result;
                }

                string[] storedFiles = Directory.GetFiles(fullFolderPath);
                string hash = "";
                List<string> hashes = [];

                // Проверяем хеши существующих файлов
                foreach (string storedFile in storedFiles)
                {
                    string fileName = Path.GetFileName(storedFile);
                    await using FileStream fileStream = new(storedFile, FileMode.Open, FileAccess.Read);

                    hash = await ComputeFileHashAsync(fileStream, cancellationToken);
                    hashes.Add(hash);
                }

                hash = _hashService.ComputeCompositeHash(hashes);

                result.IsValid = string.Equals(sourceHash, hash, StringComparison.OrdinalIgnoreCase);
                result.Message = result.IsValid ? "Все файлы целы и не были изменены" : "Некоторые файлы изменены или отсутствуют!";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка проверки файлов");
                result.Message = $"Ошибка проверки: {ex.Message}";
            }

            return result;
        }

        private async Task<string> ComputeFileHashAsync(Stream fileStream, CancellationToken cancellationToken)
        {
            fileStream.Position = 0;
            return await _hashService.ComputeFileHashAsync(fileStream, cancellationToken);
        }

        private async Task<string> ComputeFileHashAsync(IBrowserFile file, CancellationToken cancellationToken)
        {
            return await _hashService.ComputeFileHashAsync(file, cancellationToken);
        }

        /// <summary>
        /// <inheritdoc cref="BaseFileUploadService.DirectoryDelete(string)"/>
        /// </summary>
        /// <param name="relativePath"></param>
        public new void DirectoryDelete(string? relativePath)
        {
            base.DirectoryDelete(relativePath);
        }

        /// <summary>
        /// <inheritdoc cref="BaseFileUploadService.DirectoryDelete(string)"/>
        /// </summary>
        /// <remarks>Если папка содержит временую папку, то удаляет ее, иначе удаляет папку по переданному пути</remarks>
        /// <param name="relativePath"></param>
        /// <param name="HasTemp"></param>
        public void DirectoryDelete(string relativePath, bool HasTemp)
        {
            string path = HasTemp ? Path.Combine(relativePath, _settings.FolderTempName) : relativePath;
            DirectoryDelete(path);
        }

        /// <summary>
        /// Перемещает папку с файлами работы из временной папки в основную папку.
        /// </summary>
        /// <param name="sourceRelativePath">Относительный путь к исходной папке</param>
        /// <param name="destinationRelativePath">Относительный путь к целевой папке</param>
        /// <returns>Возвращает <c>true</c>, если папка успешно перемещена, иначе <c>false</c></returns>
        public override bool MoveFilesFromTemp(string sourceRelativePath, string destinationRelativePath)
        {
            try
            {
                string root = Path.Combine(_settings.FilesRootPath, _settings.FolderDataName, _settings.FolderWorksName);
                string sourceFullPath = Path.Combine(root, sourceRelativePath, _settings.FolderTempName);
                string destinationFullPath = Path.Combine(root, destinationRelativePath);
                if (!Directory.Exists(sourceFullPath))
                {
                    _logger.LogWarning("Временная папка не найдена: {SourcePath}", sourceFullPath);
                    return false;
                }
                Directory.Move(sourceFullPath, destinationFullPath);
                _logger.LogInformation("Папка успешно перемещена из {Source} в {Destination}", sourceFullPath, destinationFullPath);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка перемещения папки из {Source} в {Destination}", sourceRelativePath, destinationRelativePath);
                return false;
            }
        }

        /// <summary>
        /// Получить список файлов из указанной директории
        /// </summary>
        /// <param name="relativeDirectoryPath">Относительный путь к директории</param>
        /// <returns>Список информации о файлах</returns>
        public List<FileInfo> GetFiles(string relativeDirectoryPath)
        {
            var result = new List<FileInfo>();

            try
            {
                // Формируем полный путь
                string root = Path.Combine(_settings.FilesRootPath, _settings.FolderDataName, _settings.FolderWorksName);
                string fullPath = Path.Combine(root, relativeDirectoryPath);

                if (!Directory.Exists(fullPath))
                {
                    _logger.LogWarning("Директория не найдена: {Path}", fullPath);
                    return result;
                }

                var files = Directory.GetFiles(fullPath, "*", SearchOption.TopDirectoryOnly);

                foreach (var filePath in files)
                {
                    result.Add(new FileInfo(filePath));
                }

                _logger.LogInformation("Найдено {Count} файлов в {Path}", result.Count, relativeDirectoryPath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении списка файлов из {Path}", relativeDirectoryPath);
            }

            return result;
        }
    }

    /// <summary>
    /// Конфигурация типа файла
    /// </summary>
    public class FileTypeConfig
    {
        public string Key { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public List<string> AllowedExtensions { get; set; } = new();
        public string FilePrefix { get; set; } = string.Empty;
        public bool IsRequired { get; set; } = false;
        public string? Description { get; set; }

        public FileTypeConfig() { }

        public FileTypeConfig(string key, string displayName, List<string> allowedExtensions)
        {
            Key = key;
            DisplayName = displayName;
            AllowedExtensions = allowedExtensions;
        }
    }
}
