using ArchiveFqp.Models.Database;
using ArchiveFqp.Models.DTO.Attribute;
using System.Text.Json.Serialization;

namespace ArchiveFqp.Models.Search
{
    /// <summary>
    /// Класс, хранящий параметры поиска ВКР
    /// </summary>
    public class WorkSearchModel : SearchModel
    {
        public int? IdWork { get; set; }
        public int? IdStudent { get; set; }
        public int? IdTeacher { get; set; }
        public int? IdPost { get; set; }
        public int? IdInstitute { get; set; }
        public int? IdDepartment { get; set; }
        public int? IdDirection { get; set; }
        public int? IdProfile { get; set; }
        public int? IdWorkType { get; set; }
        public int? IdWorkStatus { get; set; }
        public int? IdWorkAccess { get; set; }
        public List<int> IdConsultants { get; set; } = [-1];
        public List<int> IdReviewers { get; set; } = [-1];
        public int? MinPages { get; set; }
        public int? MaxPages { get; set; }
        public int? MinYearDefense { get; set; }
        public int? MaxYearDefense { get; set; }
        public DateTime? MinDateAdded { get; set; }
        public DateTime? MaxDateAdded { get; set; }
        public DateTime? MinDateChanged { get; set; }
        public DateTime? MaxDateChanged { get; set; }

        // Атрибуты для поиска
        public Dictionary<int, string> SelectedAttributes { get; set; } = [];

        // Для отображения
        [JsonIgnore]
        public List<Атрибут> AllAttributes { get; set; } = [];

        [JsonIgnore]
        public List<AttributeValuesDto> AttributeValues { get; set; } = [];
    }
}
