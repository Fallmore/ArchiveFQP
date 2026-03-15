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

    /// <summary>
    /// Информация о хэшах файлов
    /// </summary>
    public class FileHashesInfo
    {
        /// <summary>
        /// Словарь хэшей файлов (имя файла -> хэш)
        /// </summary>
        public Dictionary<string, string> FileHashes { get; set; } = new();

        /// <summary>
        /// Составной хэш (ЭЦП) всех файлов
        /// </summary>
        public string CompositeHash { get; set; } = string.Empty;

        /// <summary>
        /// Время вычисления хэшей
        /// </summary>
        public DateTime CalculatedAt { get; set; }

        /// <summary>
        /// Количество файлов
        /// </summary>
        public int FileCount => FileHashes.Count;

        /// <summary>
        /// Общий размер файлов в байтах
        /// </summary>
        public long TotalSize { get; set; }
    }

    /// <summary>
    /// Результат проверки хэша
    /// </summary>
    public class HashVerificationResult
    {
        public bool IsValid { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
