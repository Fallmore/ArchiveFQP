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
		public int WorkId { get; set; } = -1;
		public int StudentId { get; set; } = -1;
		public int TeacherId { get; set; } = -1;
		public int PostId { get; set; } = -1;
		public int InstituteId { get; set; } = -1;
		public int DepartmentId { get; set; } = -1;
		public int DirectionId { get; set; } = -1;
		public int ProfileId { get; set; } = -1;
		public int WorkTypeId { get; set; } = -1;
		public int WorkStatusId { get; set; } = -1;
		public List<int> ConsultantsId { get; set; } = [-1];
		public List<int> ReviewersId { get; set; } = [-1];
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
