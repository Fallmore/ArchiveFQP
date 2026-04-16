using ArchiveFqp.Models.Settings.Department;
using ArchiveFqp.Models.Settings.Institute;

namespace ArchiveFqp.Models.Settings.SettingsArchive
{
    public class SettingsArchive
    {
        public int MaxDayWatchWorks { get; set; } = 14;
        public bool SendNotifications { get; set; }
        public bool SendNotificationsOnEmail { get; set; } 

        public string WorkApplicationsInProcessStatus { get; set; } = "В обработке";
        public string WorkApplicationsRejectStatus { get; set; } = "Отклонено";
        public string WorkApplicationsActiveStatus { get; set; } = "Активно";
        public string WorkApplicationsCompleteStatus { get; set; } = "Завершено";

        public string WorkOnReviewStatus { get; set; } = "На проверке";
        public string WorkOnProcessStatus { get; set; } = "Обработка нейросетью";
        public string WorkInArchiveStatus { get; set; } = "В архиве";
        public string WorkAcceptedStatus { get; set; } = "Принято";
        public string WorkOnReworkStatus { get; set; } = "На доработке";
        public string WorkWrittenOffStatus { get; set; } = "Списано";

        public string AttributesValueAwait { get; set; } = "Ожидание поиска...";
        public string AttributesValueNone { get; set; } = "Н/Д";
        /// <summary>
        /// Значения атрибутов, которые не относятся к данным
        /// </summary>
        public List<string> AttributesAbandonedValues = ["Н/Д", "Ожидание поиска..."];

        /// <summary>
        /// Работы, являющиеся дипломными
        /// </summary>
        public List<string> FqpWorks = ["ВКРБ", "МД"];
        /// <summary>
        /// Работы, имеющие консультантов и рецензентов
        /// </summary>
        public List<string> FqpWorksWithCR = ["МД"];

        public List<SettingsInstitute> SettingsInstitutes { get; set; } = new ();
    }
}
