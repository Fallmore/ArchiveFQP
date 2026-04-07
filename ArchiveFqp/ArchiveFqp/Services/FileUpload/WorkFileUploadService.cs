using ArchiveFqp.Models;
using ArchiveFqp.Services.Hash;
using Microsoft.AspNetCore.Components.Forms;

namespace ArchiveFqp.Services.FileUpload
{
    /// <summary>
    /// Сервис для загрузки файлов учебных работ
    /// Реализует специфические требования для загрузки различных типов файлов
    /// </summary>
    public class WorkFileUploadService : BaseFileUploadService
    {
        private readonly IHashService _hashService;
        private readonly long _maxFileSize = 100 * 1024 * 1024; // 100 MB
        private readonly string[] _allowedWordExtensions = [".doc", ".docx"];
        private readonly string[] _allowedPdfExtensions = [".pdf"];
        private readonly string[] _allowedPresentationExtensions = [".ppt", ".pptx"];
        private readonly string[] _allowedSourceCodeExtensions = [".zip", ".rar", ".7z"];
        private readonly string[] _allowedDbExtensions = [".sql", ".bak", ".backup", ".dump", ".txt"];
        private readonly string[] _allowedPasswordExtensions = [".txt"];

        public override event EventHandler<FileUploadProgressEventArgs>? ProgressChanged;

        protected virtual void OnProgressChanged(FileUploadProgressEventArgs e)
        {
            ProgressChanged?.Invoke(this, e);
        }

        public WorkFileUploadService(IWebHostEnvironment environment, IHashService hashService, ILogger<WorkFileUploadService> logger)
            : base(environment, logger)
        {
            _hashService = hashService;
        }

        public string[] GetAllowedExtensions(FileType fileType)
        {
            return fileType switch
            {
                FileType.ExplanatoryNoteWord => _allowedWordExtensions,
                FileType.ExplanatoryNotePdf => _allowedPdfExtensions,
                FileType.Presentation => _allowedPresentationExtensions,
                FileType.SourceCode => _allowedSourceCodeExtensions,
                FileType.DatabaseBackup => _allowedDbExtensions,
                FileType.PasswordFile => _allowedPasswordExtensions,
                FileType.Other => [],
                _ => []
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

            try
            {
                // Строим путь к папке
                string relativeFolderPath = BuildFolderPath(context);
#warning Раскоментировать после всех проверок
                //if (DirectoryExists(relativeFolderPath))
                //{
                //    result.FileResults = [new FileUploadResult{ 
                //        Success = false, 
                //        ErrorMessage = "Работа данного студента уже в архиве!"}];
                //    return result;
                //}
                string fullFolderPath = EnsureDirectoryExists(relativeFolderPath);

                // Загружаем все файлы и собираем их для хэширования
                List<Task<FileUploadResult>> uploadTasks = new();

                // Пояснительная записка Word
                if (workContext.ExplanatoryNoteWord != null)
                {
                    filesToHash.Add(workContext.ExplanatoryNoteWord);
                    uploadTasks.Add(UploadSpecificFile(
                        workContext.ExplanatoryNoteWord,
                        fullFolderPath,
                        relativeFolderPath,
                        FileType.ExplanatoryNoteWord,
                        "Пояснительная_записка_Word",
                        cancellationToken));
                }

                // Пояснительная записка PDF
                if (workContext.ExplanatoryNotePdf != null)
                {
                    filesToHash.Add(workContext.ExplanatoryNotePdf);
                    uploadTasks.Add(UploadSpecificFile(
                        workContext.ExplanatoryNotePdf,
                        fullFolderPath,
                        relativeFolderPath,
                        FileType.ExplanatoryNotePdf,
                        "Пояснительная_записка_PDF",
                        cancellationToken));
                }

                // Презентация
                if (workContext.Presentation != null)
                {
                    filesToHash.Add(workContext.Presentation);
                    uploadTasks.Add(UploadSpecificFile(
                        workContext.Presentation,
                        fullFolderPath,
                        relativeFolderPath,
                        FileType.Presentation,
                        "Презентация",
                        cancellationToken));
                }

                // Исходный код
                foreach (var file in workContext.SourceCodeFiles)
                {
                    filesToHash.Add(file);
                    uploadTasks.Add(UploadSpecificFile(
                        file,
                        fullFolderPath,
                        relativeFolderPath,
                        FileType.SourceCode,
                        "Исходный_код",
                        cancellationToken));
                }

                // База данных
                if (workContext.DatabaseBackup != null)
                {
                    filesToHash.Add(workContext.DatabaseBackup);
                    uploadTasks.Add(UploadSpecificFile(
                        workContext.DatabaseBackup,
                        fullFolderPath,
                        relativeFolderPath,
                        FileType.DatabaseBackup,
                        "База_данных",
                        cancellationToken));
                }

                // Файл с паролями
                if (workContext.PasswordFile != null)
                {
                    filesToHash.Add(workContext.PasswordFile);
                    uploadTasks.Add(UploadSpecificFile(
                        workContext.PasswordFile,
                        fullFolderPath,
                        relativeFolderPath,
                        FileType.PasswordFile,
                        "Пароли",
                        cancellationToken));
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

                    _logger.LogDebug("Вычислен составной хэш {Hash} для {Count} файлов",
                        result.HashesInfo.CompositeHash, result.HashesInfo.FileCount);
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка загрузки файлов для работы {WorkTitle}", context.WorkTitle);
            }

            return result;
        }

        /// <summary>
        /// Загружает файл на сервер
        /// </summary>
        /// <param name="file"></param>
        /// <param name="fullFolderPath"></param>
        /// <param name="relativeFolderPath"></param>
        /// <param name="fileType"></param>
        /// <param name="filePrefix"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>Результат загрузки файлов</returns>
        private async Task<FileUploadResult> UploadSpecificFile(
            IBrowserFile file, string fullFolderPath, string relativeFolderPath,
            FileType fileType, string filePrefix, CancellationToken cancellationToken)
        {
            FileUploadResult result = new ()
            {
                OriginalFileName = file.Name,
                FileType = fileType,
                UploadedAt = DateTime.UtcNow
            };

            string filePath = "";

            try
            {
                // Валидация файла
                if (!ValidateFile(file, fileType, out string? errorMessage))
                {
                    result.Success = false;
                    result.ErrorMessage = errorMessage;
                    return result;
                }

                // Генерация имени файла
                string extension = Path.GetExtension(file.Name);
                string storedFileName = $"{filePrefix}{extension}";

                filePath = Path.Combine(fullFolderPath, storedFileName);

                // Сохранение файла
                using Stream stream = file.OpenReadStream(_maxFileSize, cancellationToken);
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
                        FileType = fileType
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

        public override bool ValidateFile(IBrowserFile file, FileType fileType, out string? errorMessage)
        {
            errorMessage = null;

            // Проверка размера
            if (file.Size > _maxFileSize)
            {
                errorMessage = $"Файл слишком большой. Максимальный размер: {_maxFileSize / 1024 / 1024} MB";
                return false;
            }

            // Проверка расширения
            string extension = Path.GetExtension(file.Name).ToLowerInvariant();

            bool isOk = false;
            switch (fileType)
            {
                case FileType.ExplanatoryNoteWord:
                    isOk = _allowedWordExtensions.Contains(extension);
                    errorMessage = $"Для пояснительной записки в Word разрешены только {string.Join(", ", _allowedWordExtensions)} файлы";
                    break;
                case FileType.ExplanatoryNotePdf:
                    isOk = _allowedPdfExtensions.Contains(extension);
                    errorMessage = $"Для пояснительной записки в PDF разрешены только {string.Join(", ", _allowedPdfExtensions)} файлы";
                    break;
                case FileType.Presentation:
                    isOk = _allowedPresentationExtensions.Contains(extension);
                    errorMessage = $"Для презентации разрешены только {string.Join(", ", _allowedPresentationExtensions)} файлы";
                    break;
                case FileType.SourceCode:
                    isOk = _allowedSourceCodeExtensions.Contains(extension);
                    errorMessage = $"Для бэкапа БД разрешены только {string.Join(", ", _allowedSourceCodeExtensions)} файлы";
                    break;
                case FileType.DatabaseBackup:
                    isOk = _allowedDbExtensions.Contains(extension);
                    errorMessage = $"Для бэкапа БД разрешены только {string.Join(", ", _allowedDbExtensions)} файлы";
                    break;
                case FileType.PasswordFile:
                    isOk = _allowedPasswordExtensions.Contains(extension);
                    errorMessage = $"Для файла с паролями разрешены только {string.Join(", ", _allowedPasswordExtensions)} файлы";
                    break;
                case FileType.Other:
                    break;
            }

            return isOk;
        }

        public override async Task<HashVerificationResult> VerifyUploadedFilesAsync(string sourceHash, string relativeFolderPath, CancellationToken cancellationToken = default)
        {
            HashVerificationResult result = new();

            try
            {
                string fullFolderPath = Path.Combine(_baseUploadPath, relativeFolderPath);

                if (!Directory.Exists(fullFolderPath))
                {
                    result.Message = "Папка не найдена";
                    return result;
                }

                string[] storedFiles = Directory.GetFiles(fullFolderPath);
                string hash = "";
                List<string> hashes = [];

                // Проверяем хэши существующих файлов
                foreach (string storedFile in storedFiles)
                {
                    string fileName = Path.GetFileName(storedFile);
                    await using FileStream fileStream = new(storedFile, FileMode.Open, FileAccess.Read);

                    hash = await ComputeFileHashAsync(fileStream, cancellationToken);
                    hashes.Add(hash);
                }

                hash = _hashService.ComputeCompositeHash(hashes);

                result.IsValid = string.Equals(sourceHash, hash, StringComparison.OrdinalIgnoreCase); ;
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
        /// <param name="relarivePath"></param>
        public new void DirectoryDelete(string? relarivePath)
        {
            base.DirectoryDelete(relarivePath);
        }
    }
}
