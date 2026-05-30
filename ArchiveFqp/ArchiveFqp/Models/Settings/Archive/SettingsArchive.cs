using ArchiveFqp.Models.Database;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ArchiveFqp.Models.Settings.SettingsArchive
{
    public class SettingsArchive : BaseSettings
    {
        public SettingsArchive()
        {

        }

        public SettingsArchive(IDbContextFactory<ArchiveFqpContext> dbFactory, IWebHostEnvironment env)
        {
            using ArchiveFqpContext context = dbFactory.CreateDbContext();
            НастройкиУчреждения? settingsEntity = context.НастройкиУчрежденияs.FirstOrDefault();
            if (settingsEntity?.Настройки != null)
            {
                JsonSerializerSettings options = new()
                {
                    ObjectCreationHandling = ObjectCreationHandling.Replace
                };
                SettingsArchive? temp = JsonConvert.DeserializeObject<SettingsArchive>(settingsEntity.Настройки, options);
                if (temp != null)
                {
                    Copy(temp);
                }
            }

            if (string.IsNullOrWhiteSpace(FilesRootPath))
            {
                FilesRootPath = env.ContentRootPath;
            }
        }

        /// <summary>
        /// ВСЕГДА дополняйте при изменении полей, т.к. для глобального изменения
        /// сервиса просто присваивание не работает
        /// </summary>
        /// <param name="settings"></param>
        public void Copy(SettingsArchive settings)
        {
            FilesRootPath = settings.FilesRootPath;
            FolderDataName = settings.FolderDataName;
            FolderBackupsName = settings.FolderBackupsName;
            FolderTempName = settings.FolderTempName;
            FolderWorksName = settings.FolderWorksName;
            BackupSchedule = settings.BackupSchedule;
            BackupRetentionDays = settings.BackupRetentionDays;
            AutoBackupEnabled = settings.AutoBackupEnabled;
            MinDayWatchWorks = settings.MinDayWatchWorks;
            MaxDayWatchWorks = settings.MaxDayWatchWorks;
            SendNotifications = settings.SendNotifications;
            SendNotificationsOnEmail = settings.SendNotificationsOnEmail;
            RoleAdminName = settings.RoleAdminName;
            RoleInstituteResponsibleName = settings.RoleInstituteResponsibleName;
            RoleDepartmentHeadName = settings.RoleDepartmentHeadName;
            RoleDepartmentResponsibleName = settings.RoleDepartmentResponsibleName;
            RoleDepartmentClerkName = settings.RoleDepartmentClerkName;
            RoleTeacherName = settings.RoleTeacherName;
            RoleTeacherOnVerifyName = settings.RoleTeacherOnVerifyName;
            RoleStudentName = settings.RoleStudentName;
            RoleStudentOnVerifyName = settings.RoleStudentOnVerifyName;
            ApplicationTimeCheckMinutes = settings.ApplicationTimeCheckMinutes;
            ApplicationsInProcessStatus = settings.ApplicationsInProcessStatus;
            ApplicationsRejectStatus = settings.ApplicationsRejectStatus;
            ApplicationsAcceptStatus = settings.ApplicationsAcceptStatus;
            ApplicationsActiveStatus = settings.ApplicationsActiveStatus;
            ApplicationsCompleteStatus = settings.ApplicationsCompleteStatus;
            WorkOnReviewStatus = settings.WorkOnReviewStatus;
            WorkOnProcessStatus = settings.WorkOnProcessStatus;
            WorkInArchiveStatus = settings.WorkInArchiveStatus;
            WorkAcceptedStatus = settings.WorkAcceptedStatus;
            WorkOnReworkStatus = settings.WorkOnReworkStatus;
            WorkWrittenOffStatus = settings.WorkWrittenOffStatus;
            WorkAccessAll = settings.WorkAccessAll;
            WorkAccessOrganization = settings.WorkAccessOrganization;
            WorkAccessInstitute = settings.WorkAccessInstitute;
            WorkAccessDepartment = settings.WorkAccessDepartment;
            WorkAccessDirection = settings.WorkAccessDirection;
            WorkAccessProfile = settings.WorkAccessProfile;
            WorkAccessSecret = settings.WorkAccessSecret;
            AttributesValueAwait = settings.AttributesValueAwait;
            AttributesValueNone = settings.AttributesValueNone;
            AttributesAbandonedValues = settings.AttributesAbandonedValues;
            FqpWorks = settings.FqpWorks;
            FqpWorksWithCR = settings.FqpWorksWithCR;
            FqpWorksEducationLevels = settings.FqpWorksEducationLevels;
            StorageTimeWorks = settings.StorageTimeWorks;

            MaxFileSize = settings.MaxFileSize;
            AllowedFiles = settings.AllowedFiles;
            RequiredFiles = settings.RequiredFiles;

        }

        public string FilesRootPath { get; set; } = "";
        public string FolderDataName { get; set; } = "AppData";
        public string FolderTempName { get; set; } = "Temp";
        public string FolderBackupsName { get; set; } = "Backups";
        public string FolderWorksName { get; set; } = "Works";
        public long MaxFileSize { get; set; } = 100 * 1024 * 1024; // 100 MB
        
        public bool AutoBackupEnabled { get; set; } = true;
        public string BackupSchedule { get; set; } = "0 0 0 * * ?";
        public int BackupRetentionDays { get; set; } = 30;  // Сколько хранить бэкапов

        public int MinDayWatchWorks { get; set; } = 3;
        public int MaxDayWatchWorks { get; set; } = 14;
        public bool SendNotifications { get; set; } = true;
        public bool SendNotificationsOnEmail { get; set; } = true;
        public int ApplicationTimeCheckMinutes { get; set; } = 1;

        #region Для удобства доступа к данным, которые не предполагается менять в процессе работы программы
        public string RoleAdminName { get; set; } = "Администратор";
        public string RoleInstituteResponsibleName { get; set; } = "Ответственный института";
        public string RoleDepartmentHeadName { get; set; } = "Завкафедрой";
        public string RoleDepartmentResponsibleName { get; set; } = "Ответственный кафедры";
        public string RoleDepartmentClerkName { get; set; } = "Делопроизводитель";
        public string RoleTeacherName { get; set; } = "Преподаватель";
        public string RoleTeacherOnVerifyName { get; set; } = "Преподаватель на проверке";
        public string RoleStudentName { get; set; } = "Студент";
        public string RoleStudentOnVerifyName { get; set; } = "Студент на проверке";

        public string ApplicationsInProcessStatus { get; set; } = "На рассмотрении";
        public string ApplicationsRejectStatus { get; set; } = "Отклонено";
        public string ApplicationsAcceptStatus { get; set; } = "Принято";
        public string ApplicationsActiveStatus { get; set; } = "Активно";
        public string ApplicationsCompleteStatus { get; set; } = "Завершено";

        public string WorkOnReviewStatus { get; set; } = "На проверке";
        public string WorkOnProcessStatus { get; set; } = "Обработка нейросетью";
        public string WorkInArchiveStatus { get; set; } = "В архиве";
        public string WorkAcceptedStatus { get; set; } = "Принято";
        public string WorkOnReworkStatus { get; set; } = "На доработке";
        public string WorkWrittenOffStatus { get; set; } = "Списано";

        public string WorkAccessAll { get; set; } = "Все";
        public string WorkAccessOrganization { get; set; } = "Университет";
        public string WorkAccessInstitute { get; set; } = "Институт";
        public string WorkAccessDepartment { get; set; } = "Кафедра";
        public string WorkAccessDirection { get; set; } = "Направление/специальность/специализация";
        public string WorkAccessProfile { get; set; } = "Профиль";
        public string WorkAccessSecret { get; set; } = "Засекречено";

        public string AttributesValueAwait { get; set; } = "Ожидание поиска...";
        public string AttributesValueNone { get; set; } = "Н/Д";
        #endregion

        
        /// <summary>
        /// Значения атрибутов, которые не относятся к данным
        /// </summary>
        public List<string> AttributesAbandonedValues { get; set; } = ["Н/Д", "Ожидание поиска..."];

        /// <summary>
        /// Работы, являющиеся дипломными
        /// </summary>
        public List<string> FqpWorks { get; set; } = ["ВКРБ", "МД"];
        /// <summary>
        /// Работы, имеющие консультантов и рецензентов
        /// </summary>
        public List<string> FqpWorksWithCR { get; set; } = ["МД"];

        public Dictionary<string, List<string>> FqpWorksEducationLevels { get; set; } = new()
        {
            { "ВКРБ", ["Бакалавриат", "Специалитет", "Аспирантура"] },
            { "МД", ["Магистратура"] }
        };

        public Dictionary<string, int> StorageTimeWorks { get; set; } = new()
        {
            { "ВКРБ", 5},
            { "МД", 5 }
        };
    }
}
