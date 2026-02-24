using Archive.Models.Database;

namespace Archive.Models.Search
{
	/// <summary>
	/// Класс, хранящий параметры поиска
	/// </summary>
	public class SearchModel
	{
		public string SearchText { get; set; } = "";
		public int Page { get; set; } = 1;
		public int PageSize { get; set; } = 10;
		public bool AdvancedSearch { get; set; } = false;
	}
}
