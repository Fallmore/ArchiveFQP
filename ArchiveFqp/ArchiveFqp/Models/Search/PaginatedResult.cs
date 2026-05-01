namespace ArchiveFqp.Models.Search
{
    /// <summary>
    /// Класс для пагинации
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PaginatedResult<T>
    {
        public List<T> Items { get; set; } = new();
        public int TotalCount => Items.Count;
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => Math.Max(1, (int)Math.Ceiling((double)TotalCount / PageSize));
    }
}
