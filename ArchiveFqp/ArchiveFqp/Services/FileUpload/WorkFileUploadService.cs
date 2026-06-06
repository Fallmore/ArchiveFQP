using ArchiveFqp.Interfaces.FileUpload;
using ArchiveFqp.Interfaces.Hash;
using ArchiveFqp.Models.FileUpload;
using ArchiveFqp.Models.Hash;
using ArchiveFqp.Models.Settings;
using ArchiveFqp.Models.Settings.SettingsArchive;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Components.Forms;
using System.Text.RegularExpressions;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

namespace ArchiveFqp.Services.FileUpload
{
    /// <summary>
    /// Сервис для загрузки файлов учебных работ с поддержкой динамических типов
    /// </summary>
    public class WorkFileUploadService : BaseFileUploadService
    {
        private readonly IHashService _hashService;
        private readonly SettingsArchive _settings;
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

            if (string.IsNullOrEmpty(_settings.FilesRootPath))
                _settings.FilesRootPath = environment.ContentRootPath;

            _baseUploadPath = Path.Combine(_settings.FilesRootPath, _settings.FolderDataName, _settings.FolderWorksName);
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
                    FilePrefix = extensionMapping.Key,
                    IsRequired = requiredFiles != null && requiredFiles.Contains(extensionMapping.Key)
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
#warning расскоментировать после всех проверок
                //if (!workContext.IsTemp && DirectoryExists(relativeFolderPath))
                //{
                //    result.FileResults = [new FileUploadResult{
                //        Success = false,
                //        ErrorMessage = "Работа данного студента уже в архиве!"}];
                //    return result;
                //}
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
            FileTypeConfig config, CancellationToken cancellationToken)
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
                string storedFileName = $"{config.FilePrefix}{extension}";

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
        public override bool ValidateFile(IBrowserFile file, FileTypeConfig config, out string? errorMessage)
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
                string? hash = await ComputeCompositeFilesHashAsync(relativeFolderPath, cancellationToken);
                if (hash is null)
                {
                    result.Message = "Папка не найдена";
                    return result;
                }

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


        /// <summary>
        /// Вычисляет хэш файлов в директории работы
        /// </summary>
        /// <param name="relativePath"></param>
        /// <returns></returns>
        public async Task<string?> ComputeCompositeFilesHashAsync(string relativeFolderPath, CancellationToken cancellationToken = default)
        {
            string fullFolderPath = Path.Combine(_baseUploadPath, relativeFolderPath);

            if (!Directory.Exists(fullFolderPath))
                return "Папка не найдена";

            string[] storedFiles = Directory.GetFiles(fullFolderPath);
            List<string> hashes = [];

            // Проверяем хеши существующих файлов
            foreach (string storedFile in storedFiles)
            {
                string fileName = Path.GetFileName(storedFile);
                await using FileStream fileStream = new(storedFile, FileMode.Open, FileAccess.Read);

                string hash = await ComputeFileHashAsync(fileStream, cancellationToken);
                hashes.Add(hash);
            }

            return _hashService.ComputeCompositeHash(hashes);
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

        public void DeleteFile(string relativePath, string fileName)
        {
            string fullPath = Path.Combine(_baseUploadPath, relativePath, fileName);
            try
            {
                File.Delete(fullPath);
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Внимание: файл {file} не был удален: {ex}", fileName, ex.Message);
            }
        }

        public void DeleteFile(string fullName)
        {
            try
            {
                File.Delete(fullName);
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Внимание: файл {file} не был удален: {ex}", fullName, ex.Message);
            }
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
                string sourceFullPath = Path.Combine(_baseUploadPath, sourceRelativePath, _settings.FolderTempName);
                string destinationFullPath = Path.Combine(_baseUploadPath, destinationRelativePath);
                if (!Directory.Exists(sourceFullPath))
                {
                    _logger.LogWarning("Временная папка не найдена: {SourcePath}", sourceFullPath);
                    return false;
                }

                string[] files = Directory.GetFiles(sourceFullPath);

                foreach (string file in files)
                {
                    string fileName = Path.GetFileName(file);
                    string destinationFile = Path.Combine(destinationFullPath, fileName);

                    File.Move(file, destinationFile, true);
                }

                if (Directory.GetFiles(sourceFullPath).Length == 0 &&
                    Directory.GetDirectories(sourceFullPath).Length == 0)
                {
                    Directory.Delete(sourceFullPath);
                }

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
                string fullPath = Path.Combine(_baseUploadPath, relativeDirectoryPath);

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

        /// <summary>
        /// Временная загрузка файла для предварительного просмотра
        /// </summary>
        /// <param name="file">Загружаемый файл</param>
        /// <param name="fileTypeKey">Тип файла</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Информация о временном файле</returns>
        public async Task<TempFileInfo> UploadTempFileAsync(
            IBrowserFile file,
            string fileTypeKey,
            CancellationToken cancellationToken = default)
        {
            var result = new TempFileInfo();

            try
            {
                if (!_fileTypeConfigs.TryGetValue(fileTypeKey, out var config))
                {
                    result.Success = false;
                    result.ErrorMessage = $"Неизвестный тип файла: {fileTypeKey}";
                    return result;
                }

                // Валидация файла
                if (!ValidateFile(file, config, out string? errorMessage))
                {
                    result.Success = false;
                    result.ErrorMessage = errorMessage;
                    return result;
                }

                // Создаем временную директорию для сессии
                var sessionId = Guid.NewGuid().ToString();
                var tempDir = Path.Combine(_baseUploadPath, _settings.FolderTempName, sessionId);
                Directory.CreateDirectory(tempDir);

                // Генерируем имя файла
                string extension = Path.GetExtension(file.Name);
                string storedFileName = $"{config.FilePrefix}_{DateTime.Now:yyyyMMddHHmmss}{extension}";
                string tempFilePath = Path.Combine(tempDir, storedFileName);

                // Сохраняем файл
                using Stream stream = file.OpenReadStream(_settings.MaxFileSize, cancellationToken);
                using FileStream fileStream = new(tempFilePath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite, 81920, true);

                byte[] buffer = new byte[81920];
                int bytesRead;
                long totalBytesRead = 0;

                while ((bytesRead = await stream.ReadAsync(buffer, cancellationToken)) != 0)
                {
                    await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken);
                    totalBytesRead += bytesRead;

                    // Уведомляем о прогрессе
                    OnProgressChanged(new FileUploadProgressEventArgs
                    {
                        FileName = file.Name,
                        ProgressPercent = (decimal)totalBytesRead / file.Size,
                        BytesUploaded = totalBytesRead,
                        TotalBytes = file.Size,
                        FileType = config.DisplayName,
                        IsTemp = true
                    });
                }
                fileStream.Close();

                result.Success = true;
                result.SessionId = sessionId;
                result.TempFilePath = tempFilePath;
                result.RelativePath = $"{_settings.FolderTempName}/{sessionId}";
                result.OriginalFileName = file.Name;
                result.StoredFileName = storedFileName;
                result.FileSize = file.Size;
                result.FileTypeKey = fileTypeKey;
                result.FileTypeName = config.DisplayName;
                result.UploadedAt = DateTime.Now;

                if (file.Name.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                {
                    result.PageCount = await GetPdfPageCountAsync(tempFilePath);
                }

                _logger.LogDebug("Временный файл загружен: {Path}", tempFilePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка временной загрузки файла {FileName}", file.Name);
                result.Success = false;
                result.ErrorMessage = ex.Message;
            }

            return result;
        }

        /// <summary>
        /// Перемещение временного файла в постоянное хранилище
        /// </summary>
        /// <param name="tempFileInfo">Информация о временном файле</param>
        /// <param name="workPath">относительный путь к работе</param>
        /// <returns>Новый путь к файлу</returns>
        public string? MoveTempFileToWorkAsync(TempFileInfo tempFileInfo, string workPath)
        {
            if (!tempFileInfo.Success || string.IsNullOrEmpty(tempFileInfo.TempFilePath))
                return null;

            try
            {
                // Проверяем существование временного файла
                if (!File.Exists(tempFileInfo.TempFilePath))
                {
                    _logger.LogWarning("Временный файл не найден: {Path}", tempFileInfo.TempFilePath);
                    return null;
                }

                var targetDir = Path.Combine(_baseUploadPath, workPath);
                Directory.CreateDirectory(targetDir);

                // Генерируем постоянное имя файла
                string extension = Path.GetExtension(tempFileInfo.StoredFileName);
                string permanentFileName = $"{tempFileInfo.FileTypeName}{extension}";
                string targetPath = Path.Combine(targetDir, permanentFileName);

                // Перемещаем файл
                File.Move(tempFileInfo.TempFilePath, targetPath);

                // Удаляем временную директорию, если она пуста
                var tempDir = Path.GetDirectoryName(tempFileInfo.TempFilePath);
                if (tempDir != null && Directory.Exists(tempDir) && Directory.GetFiles(tempDir).Length == 0)
                {
                    Directory.Delete(tempDir);
                }

                string relativePath = $"/{workPath}/{permanentFileName}";

                _logger.LogDebug("Файл перемещен из временной в постоянную папку: {Path}", relativePath);

                return relativePath;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка перемещения временного файла");
                return null;
            }
        }

        /// <summary>
        /// Очистка временных файлов по сессии
        /// </summary>
        public void CleanupTempFilesAsync(string sessionId)
        {
            try
            {
                var tempDir = Path.Combine(_baseUploadPath, _settings.FolderTempName, sessionId);
                if (Directory.Exists(tempDir))
                {
                    Directory.Delete(tempDir, true);
                    _logger.LogDebug("Временные файлы сессии {SessionId} удалены", sessionId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка очистки временных файлов сессии {SessionId}", sessionId);
            }
        }

        /// <summary>
        /// Получение количества страниц в PDF файле с помощью PdfPig
        /// </summary>
        public async Task<int?> GetPdfPageCountAsync(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                    return null;

                await Task.Yield();

                using var document = PdfDocument.Open(filePath);
                return document.NumberOfPages;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка получения количества страниц PDF: {Path}", filePath);
                return null;
            }
        }
    }
}
