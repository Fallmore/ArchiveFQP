using ArchiveFqp.Models.Database;

namespace ArchiveFqp.Models.Search
{
	/// <summary>
	/// Класс для пагинации
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class PaginatedResult<T>
	{
		public List<T> Items { get; set; } = new();
		public int TotalCount { get; set; }
		public int Page { get; set; }
		public int PageSize { get; set; }
		public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
	}
}
