using ArchiveFqp.Interfaces.FileUpload;
using ArchiveFqp.Models.DTO.Structure;
using ArchiveFqp.Models.FileUpload;
using ArchiveFqp.Models.Hash;
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
        protected string _baseUploadPath;

        public abstract event EventHandler<FileUploadProgressEventArgs>? ProgressChanged;

        protected BaseFileUploadService(IWebHostEnvironment environment, ILogger<BaseFileUploadService> logger)
        {
            _environment = environment;
            _logger = logger;
            _baseUploadPath = Path.Combine(_environment.ContentRootPath, "AppData", "Works");
        }

        public abstract Task<FileUploadResult> UploadFileAsync(IFileUploadContext context, CancellationToken cancellationToken = default);

        public abstract Task<IEnumerable<FileUploadResult>> UploadFilesAsync(IFileUploadContext context, CancellationToken cancellationToken = default);

        public abstract bool ValidateFile(IBrowserFile file, FileTypeConfig config, out string? errorMessage);

        public abstract Task<FileUploadWithHashResult> UploadFileWithHashAsync(IFileUploadContext context, CancellationToken cancellationToken = default);

        public abstract Task<FileUploadWithHashResult> UploadFilesWithHashAsync(IFileUploadContext context, CancellationToken cancellationToken = default);

        public abstract Task<HashVerificationResult> VerifyUploadedFilesAsync(string sourceHash, string relativeFolderPath, CancellationToken cancellationToken = default);

        public abstract bool MoveFilesFromTemp(string sourceRelativePath, string destinationRelativePath);


        /// <summary>
        /// Формирует путь к папке на основе контекста
        /// </summary>
        protected virtual string BuildFolderPath(IFileUploadContext context)
        {
            // Очищаем строки от недопустимых символов
            string Sanitize(string input) => string.Join("_", input.Split(Path.GetInvalidFileNameChars()));

            string[] pathParts =
            [
                StructureDto.Abbreviate(Sanitize(context.Structure.Институт.Название), true),
                StructureDto.Abbreviate(Sanitize(context.Structure.Кафедра.Название)),
                Sanitize(context.Structure.УгснСтандарт.Название),
                StructureDto.Abbreviate(Sanitize(context.Structure.Направление.Название)),
                context.Structure.Профиль != null 
                    ? StructureDto.Abbreviate(Sanitize(context.Structure.Профиль.Название)) 
                    : "Без профиля",
                Sanitize(context.WorkType),
                context.Year.ToString(),
                Sanitize(context.StudentName),
            ];

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
