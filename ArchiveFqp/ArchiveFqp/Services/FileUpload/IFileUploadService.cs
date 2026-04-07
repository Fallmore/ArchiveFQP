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
}
