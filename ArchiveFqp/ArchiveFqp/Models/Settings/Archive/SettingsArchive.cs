using ArchiveFqp.Models.Settings.Department;
using ArchiveFqp.Models.Settings.Institute;

namespace ArchiveFqp.Models.Settings.SettingsArchive
{
    public class SettingsArchive
    {
        public int MinDayWatchWorks { get; set; } = 3;
        public int MaxDayWatchWorks { get; set; } = 14;
        public bool SendNotifications { get; set; }
        public bool SendNotificationsOnEmail { get; set; }

        public int ApplicationTimeCheckMinutes { get; set; } = 1;
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


    }
}
