using ArchiveFqp.Services.Hash;
using Microsoft.AspNetCore.Components.Forms;

namespace ArchiveFqp.Services.FileUpload
{
    /// <summary>
    /// Базовый абстрактный класс для всех сервисов загрузки
    /// </summary>
    public abstract class BaseFileUploadService : IFileUploadService
    {
        protected readonly IWebHostEnvironment _environment;
        protected readonly ILogger<BaseFileUploadService> _logger;
        protected readonly string _baseUploadPath;

        public abstract event EventHandler<FileUploadProgressEventArgs>? ProgressChanged;

        protected BaseFileUploadService(IWebHostEnvironment environment, ILogger<BaseFileUploadService> logger)
        {
            _environment = environment;
            _logger = logger;
            _baseUploadPath = Path.Combine(_environment.ContentRootPath, "AppData", "works");
        }

        public abstract Task<FileUploadResult> UploadFileAsync(IFileUploadContext context, CancellationToken cancellationToken = default);

        public abstract Task<IEnumerable<FileUploadResult>> UploadFilesAsync(IFileUploadContext context, CancellationToken cancellationToken = default);

        public abstract bool ValidateFile(IBrowserFile file, FileType fileType, out string? errorMessage);

        public abstract Task<FileUploadWithHashResult> UploadFileWithHashAsync(IFileUploadContext context, CancellationToken cancellationToken = default);
        
        public abstract Task<FileUploadWithHashResult> UploadFilesWithHashAsync(IFileUploadContext context, CancellationToken cancellationToken = default);
        
        public abstract Task<HashVerificationResult> VerifyUploadedFilesAsync(string sourceHash, string relativeFolderPath, CancellationToken cancellationToken = default);

        /// <summary>
        /// Формирует путь к папке на основе контекста
        /// </summary>
        protected virtual string BuildFolderPath(IFileUploadContext context)
        {
            // Очищаем строки от недопустимых символов
            string Sanitize(string input) => string.Join("_", input.Split(Path.GetInvalidFileNameChars()));
            string Abbreviate(string input, bool withAnd = false)
                => string.Join(string.Empty, input.Split([' ', '-'], StringSplitOptions.RemoveEmptyEntries)
                                // Убираем частицы и, в, во, или если withAnd, то пропускаем и
                                .Where(s => (withAnd && s == "и") || s.Length > 2)
                                // Оставляем код направления, если это направление, а в остальных случаях пишем аббревиатуры
                                .Select(s => (s.Length == 8 && s.Contains('.')) ? s + " " : s[..1])).ToUpper(); 

            var pathParts = new[]
            {
                Abbreviate(Sanitize(context.Institute), true),
                Abbreviate(Sanitize(context.Department)),
                Sanitize(context.UgsnStandart),
                Abbreviate(Sanitize(context.Direction)),
                Abbreviate(Sanitize(context.Profile)),
                Sanitize(context.WorkType),
                context.Year.ToString(),
                Sanitize(context.StudentName),
            };

            return Path.Combine(pathParts);
        }

        /// <summary>
        /// Создает директорию, если она не существует
        /// </summary>
        protected string EnsureDirectoryExists(string relativePath)
        {
            string fullPath = Path.Combine(_baseUploadPath, relativePath);
            if (!Directory.Exists(fullPath))
            {
                Directory.CreateDirectory(fullPath);
                    _logger.LogDebug("Создана папка: {Directory}", fullPath);
            }
            return fullPath;
        }

        protected virtual bool DirectoryExists(string relativePath)
        {
            string fullPath = Path.Combine(_baseUploadPath, relativePath);
            return Directory.Exists(fullPath);
        }

        /// <summary>
        /// Удаляет папку и файлы/папки, содержащиеся в ней
        /// </summary>
        /// <param name="relativePath">Относительный путь к папке</param>
        protected virtual void DirectoryDelete(string? relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath)) return;

            string fullPath = Path.Combine(_baseUploadPath, relativePath);
            if (Directory.Exists(fullPath))
            {
                DirectoryInfo di = new(fullPath);
                foreach (FileInfo file in di.EnumerateFiles())
                {
                    file.Delete();
                }
                di.Delete();
            }
        }

        /// <summary>
        /// Генерирует уникальное имя файла
        /// </summary>
        protected virtual string GenerateFileName(string originalFileName, FileType fileType)
        {
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string extension = Path.GetExtension(originalFileName);
            string fileNameWithoutExt = Path.GetFileNameWithoutExtension(originalFileName);

            // Очищаем имя файла от недопустимых символов
            string safeFileName = string.Join("_", fileNameWithoutExt.Split(Path.GetInvalidFileNameChars()));

            return $"{fileType}_{safeFileName}_{timestamp}{extension}";
        }
    }
}
