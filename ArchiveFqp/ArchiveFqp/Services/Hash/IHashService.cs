using Microsoft.AspNetCore.Components.Forms;

namespace ArchiveFqp.Services.Hash
{
    /// <summary>
    /// Интерфейс для вычисления и проверки хэшей файлов
    /// </summary>
    public interface IHashService
    {
        /// <summary>
        /// Вычисляет хэш файла
        /// </summary>
        Task<string> ComputeFileHashAsync(Stream fileStream, CancellationToken cancellationToken = default);

        /// <summary>
        /// Вычисляет хэш файла
        /// </summary>
        Task<string> ComputeFileHashAsync(IBrowserFile file, CancellationToken cancellationToken = default);

        /// <summary>
        /// Вычисляет хэш для коллекции файлов
        /// </summary>
        Task<string> ComputeCompositeHashAsync(IEnumerable<IBrowserFile> files, CancellationToken cancellationToken = default);

        /// <summary>
        /// Вычисляет хэш для коллекции файлов
        /// </summary>
        Task<string> ComputeCompositeHashAsync(IEnumerable<Stream> fileStreams, CancellationToken cancellationToken = default);

        /// <summary>
        /// Вычисляет хэш для коллекции хэшей
        /// </summary>
        string ComputeCompositeHash(IEnumerable<string> fileHashes);

        /// <summary>
        /// Проверяет соответствие файла хэшу
        /// </summary>
        Task<bool> VerifyFileHashAsync(Stream fileStream, string expectedHash, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Проверяет соответствие файла хэшу
        /// </summary>
        Task<bool> VerifyFileHashAsync(IBrowserFile file, string expectedHash, CancellationToken cancellationToken = default);

        /// <summary>
        /// Проверяет соответствие коллекции файлов составному хэшу
        /// </summary>
        Task<bool> VerifyCompositeHashAsync(IEnumerable<IBrowserFile> files, string expectedCompositeHash, CancellationToken cancellationToken = default);

        /// <summary>
        /// Получает информацию о хэшах файлов
        /// </summary>
        Task<FileHashesInfo> GetFileHashesInfoAsync(IEnumerable<IBrowserFile> files, CancellationToken cancellationToken = default);
    }
}
