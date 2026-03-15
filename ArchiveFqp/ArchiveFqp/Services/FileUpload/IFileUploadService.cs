using ArchiveFqp.Services.Hash;
using Microsoft.AspNetCore.Components.Forms;

namespace ArchiveFqp.Services.FileUpload
{
    /// <summary>
    /// Базовый интерфейс для всех сервисов загрузки файлов
    /// </summary>
    public interface IFileUploadService
    {
        /// <summary>
        /// Событие прогресса загрузки файла на сервер
        /// </summary>
        event EventHandler<FileUploadProgressEventArgs>? ProgressChanged;

        /// <summary>
        /// Загружает файл на сервер
        /// </summary>
        /// <param name="context"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>Результат загрузки файла</returns>
        Task<FileUploadResult> UploadFileAsync(IFileUploadContext context, CancellationToken cancellationToken = default);

        /// <summary>
        /// Загружает файлы на сервер
        /// </summary>
        /// <param name="context"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>Результаты загрузки файла</returns>
        Task<IEnumerable<FileUploadResult>> UploadFilesAsync(IFileUploadContext context, CancellationToken cancellationToken = default);

        /// <summary>
        /// Проверка файла
        /// </summary>
        /// <param name="file"></param>
        /// <param name="fileType"></param>
        /// <param name="errorMessage"></param>
        /// <returns><c>true</c>, если файл прошел проверку, в ином случае <c>false</c></returns>
        bool ValidateFile(IBrowserFile file, FileType fileType, out string? errorMessage);

        /// <summary>
        /// Загружает файл с формированием хэша содержимого файла
        /// </summary>
        /// <param name="context"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>Результат загрузки файла с хэшем</returns>
        Task<FileUploadWithHashResult> UploadFileWithHashAsync(IFileUploadContext context, CancellationToken cancellationToken = default);

        /// <summary>
        /// Загружает файлы с формированием хэша содержимого файлов
        /// </summary>
        /// <param name="context"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>Результаты загрузки файлов с хэшем</returns>
        Task<FileUploadWithHashResult> UploadFilesWithHashAsync(IFileUploadContext context, CancellationToken cancellationToken = default);
       
        /// <summary>
        /// Проверка хэша загруженных файлов с заданным хэшем
        /// </summary>
        /// <param name="sourceHash">Хэш, с которым будет проведена проверка</param>
        /// <param name="relativeFolderPath">Относительный путь к файлу</param>
        /// <param name="cancellationToken"></param>
        /// <returns>Результат проверки хэша</returns>
        Task<HashVerificationResult> VerifyUploadedFilesAsync(string sourceHash, string relativeFolderPath, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Результат загрузки файла
    /// </summary>
    public class FileUploadResult
    {
        public string OriginalFileName { get; set; } = string.Empty;
        public string StoredFileName { get; set; } = string.Empty;
        public string RelativePath { get; set; } = string.Empty;
        public string RelativePathWithoutName => RelativePath[..RelativePath.IndexOf("/" + StoredFileName)];
        public string FullPath { get; set; } = string.Empty;
        public string FullPathWithoutName => FullPath[..FullPath.IndexOf("\\" + StoredFileName)];
        public long FileSize { get; set; }
        public string ContentType { get; set; } = string.Empty;
        public FileType FileType { get; set; }
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Результат загрузки с хэшами
    /// </summary>
    public class FileUploadWithHashResult
    {
        public List<FileUploadResult> FileResults { get; set; } = new();
        public FileHashesInfo HashesInfo { get; set; } = new();
        public bool AllFilesSuccess => FileResults.All(r => r.Success);
        public int SuccessCount => FileResults.Count(r => r.Success);
        public int FailedCount => FileResults.Count(r => !r.Success);
    }

    /// <summary>
    /// Класс для отслеживания прогресса загрузки файла на сервер
    /// </summary>
    public class FileUploadProgressEventArgs : EventArgs
    {
        public string FileName { get; set; } = string.Empty;
        public decimal ProgressPercent { get; set; }
        public long BytesUploaded { get; set; }
        public long TotalBytes { get; set; }
        public FileType FileType { get; set; }
    }

    /// <summary>
    /// Типы файлов для работы
    /// </summary>
    public enum FileType
    {
        ExplanatoryNoteWord,
        ExplanatoryNotePdf,
        Report,
        Presentation,
        SourceCode,
        DatabaseBackup,
        PasswordFile,
        Other
    }
}
