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
                FilesRootPath = Path.Combine(
                    env.ContentRootPath,
                    FolderDataName,
                    FolderWorksName
                );
            }
        }

        public event Action? OnChange;
        public event EventHandler<SettingsArchive>? SettingsChanged;

        public void SaveSettings()
        {
            SettingsChanged?.Invoke(this, this);
            OnChange?.Invoke();
        }

        public void Copy(SettingsArchive settings)
        {
            FolderDataName = settings.FolderDataName;
            FolderWorksName = settings.FolderWorksName;
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
            RoleStudentName = settings.RoleStudentName;
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

            FilesRootPath = settings.FilesRootPath;
            MaxFileSize = settings.MaxFileSize;
            AllowedWordExtensions = settings.AllowedWordExtensions;
            AllowedPdfExtensions = settings.AllowedPdfExtensions;
            AllowedPresentationExtensions = settings.AllowedPresentationExtensions;
            AllowedSourceCodeExtensions = settings.AllowedSourceCodeExtensions;
            AllowedDbExtensions = settings.AllowedDbExtensions;
            AllowedPasswordExtensions = settings.AllowedPasswordExtensions;

        }

        public string FolderDataName { get; set; } = "AppData";
        public string FolderWorksName { get; set; } = "works";

        public int MinDayWatchWorks { get; set; } = 3;
        public int MaxDayWatchWorks { get; set; } = 14;
        public bool SendNotifications { get; set; }
        public bool SendNotificationsOnEmail { get; set; }
        public int ApplicationTimeCheckMinutes { get; set; } = 1;

        #region Для удобства доступа к данным, которые не предполагается менять в процессе работы программы
        public string RoleAdminName { get; set; } = "Администратор";
        public string RoleInstituteResponsibleName { get; set; } = "Ответственный института";
        public string RoleDepartmentHeadName { get; set; } = "Завкафедрой";
        public string RoleDepartmentResponsibleName { get; set; } = "Ответственный кафедры";
        public string RoleDepartmentClerkName { get; set; } = "Делопроизводитель";
        public string RoleTeacherName { get; set; } = "Преподаватель";
        public string RoleStudentName { get; set; } = "Студент";

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

        public string FileExplanatoryNoteWord { get; set; } = "Пояснительная_записка_Word";
        public string FileExplanatoryNotePDF { get; set; } = "Пояснительная_записка_PDF";
        public string FilePresentation { get; set; } = "Презентация";
        public string FileSourceCode { get; set; } = "Исходный_код";
        public string FileDb { get; set; } = "База_данных";
        public string FilePassword { get; set; } = "Пароли";
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

        public string FilesRootPath { get; set; } = ""; // 100 MB
        public long MaxFileSize { get; set; } = 100 * 1024 * 1024; // 100 MB
        public List<string> AllowedWordExtensions { get; set; } = [".doc", ".docx"];
        public List<string> AllowedPdfExtensions { get; set; } = [".pdf"];
        public List<string> AllowedPresentationExtensions { get; set; } = [".ppt", ".pptx"];
        public List<string> AllowedSourceCodeExtensions { get; set; } = [".zip", ".rar", ".7z"];
        public List<string> AllowedDbExtensions { get; set; } = [".sql", ".bak", ".backup", ".dump", ".txt"];
        public List<string> AllowedPasswordExtensions { get; set; } = [".txt"];
    }
}
