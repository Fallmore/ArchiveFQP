using ArchiveFqp.Models.Settings.SettingsArchive;
using ArchiveFqp.Services.Database;
using Npgsql;
using Quartz;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace ArchiveFqp.Jobs
{
    public class PostgresBackupJob : IJob
    {
        private readonly DatabaseBackupService _databaseBackupService;

        public PostgresBackupJob(DatabaseBackupService databaseBackupService)
        {
            _databaseBackupService = databaseBackupService;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            await _databaseBackupService.CreateBackup();
        }
    }
}
