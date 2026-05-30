using ArchiveFqp.Models.Settings.SettingsArchive;
using Npgsql;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace ArchiveFqp.Services.Database
{
    public class DatabaseBackupService
    {
        private readonly ILogger<DatabaseBackupService> _logger;
        private readonly SettingsArchive _settings;
        private readonly IConfiguration _configuration;

        public DatabaseBackupService(ILogger<DatabaseBackupService> logger,
            SettingsArchive settings, IConfiguration configuration)
        {
            _logger = logger;
            _settings = settings;
            _configuration = configuration;
        }

        public async Task CreateBackup()
        {
            _logger.LogInformation("Задача бэкапа запущена в {time}", DateTime.Now);

            if (!_settings.AutoBackupEnabled)
            {
                _logger.LogInformation("Автоматический бэкап отключен в настройках");
                return;
            }

            string? connectionString = _configuration.GetConnectionString("ArchiveFqpContext");
            var builder = new NpgsqlConnectionStringBuilder(connectionString);

            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string backupFileName = $"backup_{builder.Database}_{timestamp}.sql";
            string backupFolderPath = Path.Combine(_settings.FilesRootPath, _settings.FolderDataName, _settings.FolderBackupsName);
            if (!Directory.Exists(backupFolderPath))
                Directory.CreateDirectory(backupFolderPath);

            string backupFilePath = Path.Combine(backupFolderPath, backupFileName);
            string tempLogFile = Path.Combine(backupFolderPath, $"pg_dump_log_{timestamp}.txt");

            string dumpCommand = $"pg_dump -h {builder.Host} -p {builder.Port} "
                                + $"-U {builder.Username} -d {builder.Database} "
                                + $"-f \"{backupFilePath}\" 2> \"{tempLogFile}\"";

            bool isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
            bool isLinux = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
            bool isMacOS = RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

            string fileName;
            string arguments;

            if (isWindows)
            {
                fileName = "cmd.exe";
                arguments = $"/c \"{dumpCommand}\"";
            }
            else if (isLinux || isMacOS)
            {
                fileName = "/bin/bash";
                arguments = $"-c \"{dumpCommand}\"";
            }
            else
            {
                _logger.LogError("Операционная система не поддерживается");
                return;
            }

            var startInfo = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                RedirectStandardInput = true,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            // Альтернативный способ: передать пароль через переменную окружения.
            startInfo.EnvironmentVariables["PGPASSWORD"] = builder.Password;

            try
            {
                using var process = new Process { StartInfo = startInfo };
                process.Start();
                string output = await process.StandardOutput.ReadToEndAsync();
                string error = await process.StandardError.ReadToEndAsync();
                await process.WaitForExitAsync();

                if (process.ExitCode == 0)
                {
                    _logger.LogInformation("Бэкап успешно создан: {FilePath} (размер: {Size} MБ)",
                        backupFilePath,
                        (float)(new FileInfo(backupFilePath).Length) / 1024 / 1024);

                    await CleanupOldBackupsAsync(backupFolderPath, _settings.BackupRetentionDays);

                    if (File.Exists(tempLogFile))
                        File.Delete(tempLogFile);
                }
                else
                {
                    _logger.LogError("Произошла ошибка при создании бэкапа. Код ошибки: {ExitCode}", process.ExitCode);
                    if (File.Exists(tempLogFile))
                    {
                        var logContent = await File.ReadAllTextAsync(tempLogFile);
                        _logger.LogError("Лог ошибок: {LogContent}", logContent);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Не удалось выполнить команду pg_dump");
            }
        }

        private async Task CleanupOldBackupsAsync(string backupFolder, int retentionDays)
        {
            try
            {
                var cutoffDate = DateTime.Now.AddDays(-retentionDays);
                var oldBackups = Directory.GetFiles(backupFolder, "backup_*.sql")
                    .Select(f => new FileInfo(f))
                    .Where(f => f.CreationTime < cutoffDate);

                foreach (var backup in oldBackups)
                {
                    backup.Delete();
                    _logger.LogDebug("Удален старый бэкап: {FileName}", backup.Name);
                }

                _logger.LogInformation("Очистка старых бэкапов завершена. Удалено {Count} файлов", oldBackups.Count());
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Ошибка при очистке старых бэкапов: {Message}", ex.Message);
            }
        }
    }
}
