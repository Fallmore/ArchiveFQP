using ArchiveFqp.Interfaces.Hash;
using ArchiveFqp.Models.Hash;
using Microsoft.AspNetCore.Components.Forms;
using System.Security.Cryptography;

namespace ArchiveFqp.Services.Hash
{
    /// <summary>
    /// Реализация сервиса хэширования на основе SHA-256
    /// </summary>
    public class Sha256HashService : IHashService
    {
        private readonly ILogger<Sha256HashService> _logger;

        public Sha256HashService(ILogger<Sha256HashService> logger)
        {
            _logger = logger;
        }

        public async Task<string> ComputeFileHashAsync(Stream fileStream, CancellationToken cancellationToken = default)
        {
            try
            {
                using SHA256 sha256 = SHA256.Create();
                byte[] hash = await sha256.ComputeHashAsync(fileStream, cancellationToken);
                return ConvertToHexString(hash);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка вычисления хэша файла");
                throw;
            }
        }

        public async Task<string> ComputeFileHashAsync(IBrowserFile file, CancellationToken cancellationToken = default)
        {
            try
            {
                using Stream stream = file.OpenReadStream(100 * 1024 * 1024, cancellationToken); // 100 MB макс
                return await ComputeFileHashAsync(stream, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка вычисления хэша файла {FileName}", file.Name);
                throw;
            }
        }

        public async Task<string> ComputeCompositeHashAsync(IEnumerable<IBrowserFile> files, CancellationToken cancellationToken = default)
        {
            List<string> fileHashes = new();

            foreach (Stream file in files)
            {
                string fileHash = await ComputeFileHashAsync(file, cancellationToken);
                fileHashes.Add(fileHash);
            }

            return ComputeCompositeHash(fileHashes);
        }

        public async Task<string> ComputeCompositeHashAsync(IEnumerable<Stream> fileStreams, CancellationToken cancellationToken = default)
        {
            List<string> fileHashes = new();

            foreach (Stream file in fileStreams)
            {
                string fileHash = await ComputeFileHashAsync(file, cancellationToken);
                fileHashes.Add(fileHash);
            }

            return ComputeCompositeHash(fileHashes);
        }


        public string ComputeCompositeHash(IEnumerable<string> fileHashes)
        {
            // Сортируем хэши для детерминированного результата
            List<string> sortedHashes = fileHashes.OrderBy(h => h).ToList();
            List<byte> combinedBytes = new();

            foreach (string hash in sortedHashes)
            {
                byte[] hashBytes = ConvertFromHexString(hash);
                combinedBytes.AddRange(hashBytes);
            }

            byte[] compositeHash = SHA256.HashData(combinedBytes.ToArray());
            return ConvertToHexString(compositeHash);
        }

        public async Task<bool> VerifyFileHashAsync(Stream fileStream, string expectedHash, CancellationToken cancellationToken = default)
        {
            try
            {
                string computedHash = await ComputeFileHashAsync(fileStream, cancellationToken);
                return string.Equals(computedHash, expectedHash, StringComparison.OrdinalIgnoreCase);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка проверки хэша файла");
                return false;
            }
        }

        public async Task<bool> VerifyFileHashAsync(IBrowserFile file, string expectedHash, CancellationToken cancellationToken = default)
        {
            try
            {
                string computedHash = await ComputeFileHashAsync(file, cancellationToken);
                return string.Equals(computedHash, expectedHash, StringComparison.OrdinalIgnoreCase);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка проверки хэша файла");
                return false;
            }
        }

        public async Task<bool> VerifyCompositeHashAsync(IEnumerable<IBrowserFile> files, string expectedCompositeHash, CancellationToken cancellationToken = default)
        {
            try
            {
                string computedCompositeHash = await ComputeCompositeHashAsync(files, cancellationToken);
                return string.Equals(computedCompositeHash, expectedCompositeHash, StringComparison.OrdinalIgnoreCase);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка проверки составного хэша");
                return false;
            }
        }

        public async Task<FileHashesInfo> GetFileHashesInfoAsync(IEnumerable<IBrowserFile> files, CancellationToken cancellationToken = default)
        {
            FileHashesInfo info = new ()
            {
                CalculatedAt = DateTime.UtcNow
            };

            List<string> fileHashes = new ();
            long totalSize = 0;

            foreach (IBrowserFile file in files)
            {
                string hash = await ComputeFileHashAsync(file, cancellationToken);
                info.FileHashes[file.Name] = hash;
                fileHashes.Add(hash);
                totalSize += file.Size;
            }

            info.TotalSize = totalSize;
            info.CompositeHash = ComputeCompositeHash(fileHashes);

            return info;
        }

        private static string ConvertToHexString(byte[] bytes)
        {
            return Convert.ToHexString(bytes).ToLowerInvariant();
        }

        private static byte[] ConvertFromHexString(string hex)
        {
            return Convert.FromHexString(hex);
        }
    }
}
