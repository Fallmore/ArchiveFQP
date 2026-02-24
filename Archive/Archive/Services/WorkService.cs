using Archive.Models.Database;
using Archive.Models.Search;
using Archive.Properties;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Archive.Services
{
	/// <summary>
	/// Сервис взаимодействия с работами
	/// </summary>
	public interface IWorkService
	{
		/// <summary>
		/// Поиск работ
		/// </summary>
		/// <param name="searchModel">Параметры поиска</param>
		/// <returns></returns>
		Task<PaginatedResult<Работа>> SearchWorksAsync(WorkSearchModel searchModel);

		/// <summary>
		/// Получение справочника типов работ
		/// </summary>
		/// <returns></returns>
		Task<List<ТипРаботы>> GetWorkTypesAsync();

		/// <summary>
		/// Получение справочника статусов работ
		/// </summary>
		/// <returns></returns>
		Task<List<СтатусРаботы>> GetWorkStatusesAsync();

		/// <summary>
		/// Получение студентов, имеющие работы
		/// </summary>
		/// <returns></returns>
		Task<List<Студент>> GetStudentsAsync();

		/// <summary>
		/// Получение преподавателей, руководящиих ВКР
		/// </summary>
		/// <returns></returns>
		Task<List<Преподаватель>> GetTeachersAsync();

		/// <summary>
		/// Получение справочника консультантов
		/// </summary>
		/// <returns></returns>
		Task<List<Консультант>> GetConsultantsAsync();

		/// <summary>
		/// Получение справочника рецензентов
		/// </summary>
		/// <returns></returns>
		Task<List<Рецензент>> GetReviewersAsync();

		/// <summary>
		/// Получение консультантов работы
		/// </summary>
		/// <returns></returns>
		Task<List<Консультант>> GetConsultantsAsync(Работа work);

		/// <summary>
		/// Получение рецензентов работы
		/// </summary>
		/// <returns></returns>
		Task<List<Рецензент>> GetReviewersAsync(Работа work);

		/// <summary>
		/// Получение справочника атрибутов
		/// </summary>
		/// <returns></returns>
		Task<List<Атрибут>> GetAllAttributesAsync();

		/// <summary>
		/// Получение справочника значений для каждого атрибута
		/// </summary>
		/// <param name="attrs">Список атрибутов</param>
		/// <returns>Словарь idАтрибута-значение</returns>
		Task<Dictionary<int, List<string>>> GetAttributeValuesAsync(List<Атрибут>? attrs = null);

		/// <summary>
		/// Получение значений атрибутов у работ
		/// </summary>
		/// <param name="works"></param>
		/// <returns>Словарь idРаботы-(Словарь атрибут-значение)</returns>
		Task<Dictionary<int, Dictionary<string, string>>> GetWorksAttributesAsync(List<Работа> works);

		/// <summary>
		/// В зависимости от типа работы возвращает год
		/// </summary>
		/// <param name="work"></param>
		/// <returns>Если работа относится к ВКР, то год окончания обучения студентом, 
		/// иначе год добавления работы</returns>
		int PickDateWork(Работа work);
	}
	public class WorkService : IWorkService
	{
		private readonly IDbContextFactory<ArchiveFqpContext> _dbFactory;

		// Настройка сериалайзера для кириллицы
		private readonly JsonSerializerOptions _options = new JsonSerializerOptions
		{
			Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
			WriteIndented = true
		};

		// Значения атрибутов, которые не надо выбирать
		private readonly string[] abandonedValues = ["Н/Д", "Ожидание поиска..."];
		// Работы, являющиеся ВКР
		private readonly string[] fqpWorks = ["ВКРБ", "МД"];

		public WorkService(IDbContextFactory<ArchiveFqpContext> dbFactory)
		{
			_dbFactory = dbFactory;
		}

		public async Task<PaginatedResult<Работа>> SearchWorksAsync(WorkSearchModel searchModel)
		{
			using ArchiveFqpContext context = _dbFactory.CreateDbContext();

			// Преобразуем атрибуты в JSON
			string? attributesJson = null;

			if (searchModel.SelectedAttributes.Any(a => !string.IsNullOrEmpty(a.Value)))
			{
				var dict = searchModel.SelectedAttributes
					.Where(a => !string.IsNullOrEmpty(a.Value))
					.ToDictionary(a => a.Key.ToString(), a => a.Value);

				attributesJson = JsonSerializer.Serialize(dict, _options);
			}

			// Вызов функции PostgreSQL
			List<Работа>? works = await context.Работаs
				.FromSqlRaw(@"SELECT * FROM поиск_работы({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, 
				{8}, {9}, {10}, {11}, {12}, {13}, {14}, {15}, {16}, {17}::timestamp, {18}::timestamp,
				{19}::timestamp, {20}::timestamp, {21}::json)",
					searchModel.SearchText ?? "",
					searchModel.InstituteId,
					searchModel.DepartmentId,
					searchModel.DirectionId,
					searchModel.ProfileId,
					searchModel.WorkId,
					searchModel.StudentId,
					searchModel.TeacherId,
					searchModel.PostId,
					searchModel.ConsultantsId.Length == 0 ? [-1] : searchModel.ConsultantsId,
					searchModel.ReviewersId.Length == 0 ? [-1] : searchModel.ReviewersId,
					searchModel.MinPages ?? -1,
					searchModel.MaxPages ?? -1,
					searchModel.WorkTypeId,
					searchModel.WorkStatusId,
					searchModel.MinYearDefense ?? -1,
					searchModel.MaxYearDefense ?? -1,
					searchModel.MinDateAdded,
					searchModel.MaxDateAdded,
					searchModel.MinDateChanged,
					searchModel.MaxDateChanged,
					attributesJson)
				.AsNoTracking()
				.Include("IdПреподавателяNavigation.IdПользователяNavigation")
				.Include("IdПреподавателяNavigation.IdДолжностиNavigation")
				.Include("IdСтудентаNavigation.IdПользователяNavigation")
				.Include("IdСтудентаNavigation.IdИнститутаNavigation")
				.Include("IdСтудентаNavigation.IdУровняОбразованияNavigation")
				.Include("IdСтудентаNavigation.IdФормыОбразованияNavigation")
				.Include("IdСтудентаNavigation.IdНаправленияNavigation.IdКафедрыNavigation.IdУгснNavigation.IdУгснСтандартаNavigation")
				.Include(x => x.IdТипаРаботыNavigation)
				.Include(x => x.IdСтатусаРаботыNavigation)
                .Include(w => w.ВыдачаРаботыs)
                .Include(w => w.ОценкаПреподавателяs)
				.AsSingleQuery()
				.OrderByDescending(x => x.ДатаДобавления)
				.ToListAsync();

			return new PaginatedResult<Работа>
			{
				Items = works,
				TotalCount = works.Count(),
				Page = searchModel.Page,
				PageSize = searchModel.PageSize
			};
		}

		public async Task<List<ТипРаботы>> GetWorkTypesAsync()
		{
			using ArchiveFqpContext context = _dbFactory.CreateDbContext();
			return await context.ТипРаботыs.ToListAsync();
		}

		public async Task<List<СтатусРаботы>> GetWorkStatusesAsync()
		{
			using ArchiveFqpContext context = _dbFactory.CreateDbContext();
			return await context.СтатусРаботыs.ToListAsync();
		}

		public async Task<List<Студент>> GetStudentsAsync()
		{
			using ArchiveFqpContext context = _dbFactory.CreateDbContext();
			return [.. (await context.Студентs
				.Include(s => s.IdПользователяNavigation)
				.OrderBy(u => u.IdПользователяNavigation.Фамилия)
				.ToListAsync())
				.DistinctBy(s => s.IdСтудента)];
		}

		public async Task<List<Преподаватель>> GetTeachersAsync()
		{
			using ArchiveFqpContext context = _dbFactory.CreateDbContext();
			return [.. (await context.Преподавательs
				.Include(t => t.IdПользователяNavigation)
				.OrderBy(u => u.IdПользователяNavigation.Фамилия)
				.ToListAsync())
				.DistinctBy(t => t.IdПреподавателя)];
		}

		public async Task<List<Консультант>> GetConsultantsAsync()
		{
			using ArchiveFqpContext context = _dbFactory.CreateDbContext();
			return await context.Консультантs
				.FromSqlRaw("SELECT * FROM ONLY \"консультант\"")
				.Include(c => c.IdДолжностиNavigation)
				.Include(c => c.IdПреподавателяNavigation)
					.ThenInclude(t => t.IdПользователяNavigation)
				.ToListAsync();
		}

		public async Task<List<Рецензент>> GetReviewersAsync()
		{
			using ArchiveFqpContext context = _dbFactory.CreateDbContext();
			return await context.Рецензентs
				.FromSqlRaw("SELECT * FROM ONLY \"рецензент\"")
				.Include(c => c.IdДолжностиNavigation)
				.Include(c => c.IdПреподавателяNavigation)
					.ThenInclude(t => t.IdПользователяNavigation)
				.ToListAsync();
		}

		public async Task<List<Консультант>> GetConsultantsAsync(Работа work)
		{
			return (await GetConsultantsAsync())
				.Where(r => r.IdРаботы == work.IdРаботы)
				.ToList();
		}

		public async Task<List<Рецензент>> GetReviewersAsync(Работа work)
		{
			return (await GetReviewersAsync())
				.Where(r => r.IdРаботы == work.IdРаботы)
				.ToList();
		}

		public async Task<List<Атрибут>> GetAllAttributesAsync()
		{
			using ArchiveFqpContext context = _dbFactory.CreateDbContext();
			return await context.Атрибутs.OrderBy(a => a.Название).ToListAsync();
		}

		public async Task<Dictionary<int, List<string>>> GetAttributeValuesAsync(List<Атрибут>? attrs)
		{
			using ArchiveFqpContext? context = _dbFactory.CreateDbContext();

			List<Атрибут> attributes = attrs is null ? await context.Атрибутs.ToListAsync() : attrs;
			Dictionary<int, List<string>> result = [];

			foreach (Атрибут attribute in attributes)
			{
				List<string> values = await context.ДанныеПоАтрибs
				.Join(context.АтрибутУчрежденияs, d => d.IdСтруктуры, a => a.IdСтруктуры,
					(d, a) => new { a.IdАтрибута, d.Данные })
				.Where(d => d.IdАтрибута == attribute.IdАтрибута)
				.Select(d => d.Данные)
				.Distinct()
				.Where(d => !abandonedValues.Contains(d))
				.ToListAsync();

				result[attribute.IdАтрибута] = values;
			}

			return result;
		}

		public async Task<Dictionary<int, Dictionary<string, string>>> GetWorksAttributesAsync(List<Работа> works)
		{
			Dictionary<int, Dictionary<string, string>> result = [];

			if (works.Count == 0) return result;

			using ArchiveFqpContext? context = _dbFactory.CreateDbContext();

			List<int> workIds = works.Select(w => w.IdРаботы).ToList();

			var data = await context.АтрибутУчрежденияs
				.Include(ad => ad.IdАтрибутаNavigation)
				.Join(context.ДанныеПоАтрибs, a => a.IdСтруктуры, d => d.IdСтруктуры,
					(a, d) => new { a.IdАтрибутаNavigation.Название, d.Данные, d.IdРаботы })
				.Where(ad => workIds.Contains(ad.IdРаботы)
					&& !abandonedValues.Contains(ad.Данные))
				.ToListAsync();

			if (data?.Count == 0 || data is null) return result;
			
			result = data
				.GroupBy(ad => ad.IdРаботы)
				.ToDictionary(g => g.Key,
				d => d.ToDictionary(
					d => d.Название,
					d => d.Данные)
				);

			return result;
		}

		public int PickDateWork(Работа work)
		{
			if (fqpWorks.Contains(work.IdТипаРаботыNavigation.Название))
				return work.IdСтудентаNavigation.ГодОкончания;
			else return work.ДатаДобавления.Year;
		}
	}

}
