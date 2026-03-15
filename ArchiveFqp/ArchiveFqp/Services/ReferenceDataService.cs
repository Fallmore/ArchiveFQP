using ArchiveFqp.Models.Database;
using Microsoft.EntityFrameworkCore;

namespace ArchiveFqp.Services
{
	/// <summary>
	/// Класс со списками справочников БД
	/// </summary>
	public class ReferenceDataService
	{
		public List<Атрибут> Attributes { get; set; } = [];
		public List<Должность> Posts { get; set; } = [];
		public List<ДоступРаботы> AccessWork { get; set; } = [];
		public List<Институт> Institutes { get; set; } = [];
		public List<Кафедра> Departments { get; set; } = [];
		public List<Консультант> Consultants { get; set; } = [];
		public List<Направление> Directions { get; set; } = [];
		public List<Преподаватель> Teachers { get; set; } = [];
		public List<Профиль> Profiles { get; set; } = [];
		public List<Рецензент> Reviewers { get; set; } = [];
		public List<РольПользователя> RoleUsers { get; set; } = [];
		public List<СтатусРаботы> WorkStatuses { get; set; } = [];
		public List<Студент> Students { get; set; } = [];
		public List<ТипРаботы> WorkTypes { get; set; } = [];
		public List<Угсн> Ugsns { get; set; } = [];
		public List<УгснСтандарт> UgsnStandarts { get; set; } = [];
		public List<УровеньОбразования> EducationLevels { get; set; } = [];
		public List<ФормаОбучения> EducationForms { get; set; } = [];
		

		public DateTime LastUpdated { get; set; }

		public bool IsExpired()
		{
			return (DateTime.Now - LastUpdated).TotalMinutes > 30; // Обновляем каждые 30 минут
		}

		public async Task LoadReferenceData(IDbContextFactory<ArchiveFqpContext> DbFactory)
		{
			using var context = DbFactory.CreateDbContext();

			Students = await context.Студентs
				.Include(s => s.IdПользователяNavigation)
				.OrderBy(s => s.IdПользователяNavigation.Фамилия)
				.ToListAsync();

			Teachers = await context.Преподавательs
				.Include(t => t.IdПользователяNavigation)
				.Include(t => t.IdДолжностиNavigation)
				.OrderBy(t => t.IdПользователяNavigation.Фамилия)
				.ToListAsync();

			Ugsns = await context.Угснs
				.OrderBy(u => u.Название)
				.ToListAsync();

			UgsnStandarts = await context.УгснСтандартs
				.OrderBy(s => s.Название)
				.ToListAsync();

			Directions = await context.Направлениеs
				.Include(d => d.IdКафедрыNavigation)
				.OrderBy(n => n.Название)
				.ToListAsync();

			Profiles = await context.Профильs
				.OrderBy(p => p.Название)
				.ToListAsync();

			EducationLevels = await context.УровеньОбразованияs
				.OrderBy(e => e.Название)
				.ToListAsync();

			EducationForms = await context.ФормаОбученияs
				.OrderBy(e => e.Название)
				.ToListAsync();

			Institutes = await context.Институтs
				.OrderBy(i => i.Название)
				.ToListAsync();

			Departments = await context.Кафедраs
				.OrderBy(d => d.Название)
				.ToListAsync();

			WorkTypes = await context.ТипРаботыs
				.OrderBy(t => t.Название)
				.ToListAsync();
		}

	}
}
