using ArchiveFqp.Models.Hash;
using Microsoft.AspNetCore.Components.Forms;

namespace ArchiveFqp.Interfaces.Hash
{
    /// <summary>
    /// Интерфейс для вычисления и проверки хешей файлов
    /// </summary>
    public interface IHashService
    {
        /// <summary>
        /// Вычисляет хеш файла
        /// </summary>
        Task<string> ComputeFileHashAsync(Stream fileStream, CancellationToken cancellationToken = default);

        /// <summary>
        /// Вычисляет хеш файла
        /// </summary>
        Task<string> ComputeFileHashAsync(IBrowserFile file, CancellationToken cancellationToken = default);

        /// <summary>
        /// Вычисляет хеш для коллекции файлов
        /// </summary>
        Task<string> ComputeCompositeHashAsync(IEnumerable<IBrowserFile> files, CancellationToken cancellationToken = default);

        /// <summary>
        /// Вычисляет хеш для коллекции файлов
        /// </summary>
        Task<string> ComputeCompositeHashAsync(IEnumerable<Stream> fileStreams, CancellationToken cancellationToken = default);

        /// <summary>
        /// Вычисляет хеш для коллекции хешей
        /// </summary>
        string ComputeCompositeHash(IEnumerable<string> fileHashes);

        /// <summary>
        /// Проверяет соответствие файла хешу
        /// </summary>
        Task<bool> VerifyFileHashAsync(Stream fileStream, string expectedHash, CancellationToken cancellationToken = default);

        /// <summary>
        /// Проверяет соответствие файла хешу
        /// </summary>
        Task<bool> VerifyFileHashAsync(IBrowserFile file, string expectedHash, CancellationToken cancellationToken = default);

        /// <summary>
        /// Проверяет соответствие коллекции файлов составному хешу
        /// </summary>
        Task<bool> VerifyCompositeHashAsync(IEnumerable<IBrowserFile> files, string expectedCompositeHash, CancellationToken cancellationToken = default);

        /// <summary>
        /// Получает информацию о хешах файлов
        /// </summary>
        Task<FileHashesInfo> GetFileHashesInfoAsync(IEnumerable<IBrowserFile> files, CancellationToken cancellationToken = default);
    }
}
