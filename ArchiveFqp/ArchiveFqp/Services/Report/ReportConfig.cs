using ArchiveFqp.Models.Search;

namespace ArchiveFqp.Services.Report
{
    public class ReportConfig
    {
        public string ReportTitle { get; set; } = "Отчет по работам";

        // Основные настройки
        public bool IncludeStatistics { get; set; } = true;
        public bool IncludeDistribution { get; set; } = true;
        public bool IncludeWorksList { get; set; } = true;
        public string ListTitle { get; set; } = "Список работ";

        // Настройки отображения в таблице
        public bool ShowWorkId { get; set; } = true;
        public bool ShowTitle { get; set; } = true;
        public bool ShowStudent { get; set; } = true;
        public bool ShowSupervisor { get; set; } = true;
        public bool ShowType { get; set; } = true;
        public bool ShowStatus { get; set; } = true;
        public bool ShowAccess { get; set; } = true;
        public bool ShowPages { get; set; } = true;
        public bool ShowDateAdd { get; set; } = true;
        public bool ShowDateChange { get; set; } = true;
        public bool ShowYearDefence { get; set; } = true;
        public bool ShowYearDefense { get; set; } = true;
        public bool ShowConsultants { get; set; } = true;
        public bool ShowReviewers { get; set; } = true;
        public bool ShowAttributes { get; set; } = false;

        // Группировка для статистики
        public bool GroupByWorkType { get; set; } = true;
        public bool GroupByStatus { get; set; } = true;
        public bool GroupByYear { get; set; } = true;
        public bool GroupByInstitute { get; set; } = true;
        public bool GroupByDepartment { get; set; } = true;
        public bool GroupByDirection { get; set; } = true;
        public bool GroupByProfile { get; set; } = true;
        public bool GroupBySupervisor { get; set; } = true;

        // Выбранные атрибуты для отчета
        public List<int> SelectedAttributes { get; set; } = new();

        // Фильтры для данных
        public Dictionary<string, object> Filters { get; set; } = new();
    }
}
