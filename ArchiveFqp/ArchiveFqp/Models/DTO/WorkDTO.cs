using ArchiveFqp.Models.Database;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ArchiveFqp.Models.DTO.Work
{
	public class WorkDTO
	{
		[Required(ErrorMessage = "Тема обязательна")]
		public string Тема { get; set; } = "";

		[Required(ErrorMessage = "Выберите студента")]
		public int? IdСтудента { get; set; }

		[Required(ErrorMessage = "Выберите руководителя")]
		public int? IdПреподавателя { get; set; }

		public List<int> IdКонсультанта { get; set; } = [-1];
		public List<int> IdРецензента { get; set; } = [-1];

		[Required(ErrorMessage = "Выберите УГСН")]
		public int? IdУгсн { get; set; }

		[Required(ErrorMessage = "Выберите стандарт")]
		public int? IdУгснСтандарта { get; set; }

		[Required(ErrorMessage = "Выберите направление")]
		public int? IdНаправления { get; set; }

		public int? IdПрофиля { get; set; }

		[Required(ErrorMessage = "Выберите уровень образования")]
		public int? IdУровняОбразования { get; set; }

		[Required(ErrorMessage = "Выберите форму обучения")]
		public int? IdФормыОбучения { get; set; }

		[Required(ErrorMessage = "Выберите институт")]
		public int? IdИнститута { get; set; }

		[Required(ErrorMessage = "Выберите кафедру")]
		public int? IdКафедры { get; set; }

		public int? ГодВыпуска { get; set; }

		public string? Аннотация { get; set; }

		[Required(ErrorMessage = "Укажите количество страниц")]
		[Range(1, 300, ErrorMessage = "Количество страниц должно быть от 1 до 300")]
		public int? КоличСтраниц { get; set; }

		[Required(ErrorMessage = "Выберите тип работы")]
		public int? IdТипаРаботы { get; set; }

        [Required(ErrorMessage = "Выберите доступ работы")]
        public int? IdДоступаРаботы { get; set; }

        public int IdСтатусаРаботы { get; set; } = 3;
#warning Поменять константу на точный ID статуса работы

		[JsonIgnore]
		public List<Студент> Students = [];
		[JsonIgnore]
		public List<Преподаватель> Teachers = [];
		[JsonIgnore]
        public List<Угсн> Ugsns = [];
		[JsonIgnore]
        public List<УгснСтандарт> UgsnStandarts = [];
		[JsonIgnore]
        public List<Направление> Directions = [];
		[JsonIgnore]
        public List<Профиль> Profiles = [];
		[JsonIgnore]
        public List<УровеньОбразования> EducationLevels = [];
		[JsonIgnore]
        public List<ФормаОбучения> EducationForms = [];
        [JsonIgnore]
        public List<Институт> Institutes = [];
		[JsonIgnore]
        public List<Кафедра> Departments = [];
		[JsonIgnore]
        public List<ТипРаботы> WorkTypes = [];
        [JsonIgnore]
        public List<ДоступРаботы> WorkAccess = [];

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

            WorkAccess = await context.ДоступРаботыs
                .OrderBy(t => t.Название)
                .ToListAsync();
        }

		/// <summary>
		/// Устанавливает ID студента и заполняет на основе его данных данные о работе:
		/// <br>Уровень образования, форма обучения</br>
		/// <br>Институт, кафедра, направление, профиль</br>
		/// <br>Год выпуска, УГСН и стандарт УГСН</br> 
		/// </summary>
		/// <param name="idStudent"></param>
		/// <returns><c>true</c>, если все данные найдены,
		/// <br><c>false</c> - в обратном случае или если нет ID </br></returns>
		public bool SetStudent(int? idStudent)
		{
			bool nulify()
			{
				IdУровняОбразования = IdФормыОбучения = IdПрофиля = ГодВыпуска = null;
				// Обнуляем остальные связанные данные вверх по иерархии учреждения
				// студент -> направление (профиль пропускается, т.к. он может быть null
				// и это норма)
				return (SetDirection(-1));
			}

			if (!idStudent.HasValue)
			{
				return nulify();
			}

			Студент? student = Students.FirstOrDefault(s => s.IdСтудента == idStudent.Value);
			if (student != null)
			{
				IdУровняОбразования = student.IdУровняОбразования;
				IdФормыОбучения = student.IdФормыОбучения;
				IdПрофиля = student.IdПрофиля;
				ГодВыпуска = student.ГодОкончания;

				bool isOk = SetDirection(student.IdНаправления);
				if (!isOk)
				{
					IdУровняОбразования = IdФормыОбучения = IdПрофиля = ГодВыпуска = null;
				}

				return isOk;
			}

			return nulify();
		}

		/// <summary>
		/// Устанавливает ID профиля и заполняет на основе его данных данные о работе:
		/// <br>Институт, кафедра, направление</br> 
		/// <br>УГСН и стандарт УГСН</br>
		/// </summary>
		/// <param name="idProfile"></param>
		/// <returns><c>true</c>, если все данные найдены,
		/// <br><c>false</c> - в обратном случае или если нет ID </br></returns>
		public bool SetProfile(int? idProfile)
		{
			bool nulify()
			{
				IdПрофиля = null;
				// Не идём вверх по иерархии, т.к. профиль
				// может быть null и это норма
				return false;
			}

			if (!idProfile.HasValue)
			{
				return nulify();
			}

			Профиль? profile = Profiles.FirstOrDefault(s => s.IdПрофиля == idProfile.Value);
			if (profile != null)
			{
				IdПрофиля = profile.IdПрофиля;

				bool isOk = SetDirection(profile.IdНаправления);
				if (!isOk)
				{
					IdПрофиля = null;
				}

				return isOk;
			}

			return nulify();
		}

		/// <summary>
		/// Устанавливает ID направления и заполняет на основе его данных данные о работе:
		/// <br>Институт, кафедра, УГСН и стандарт УГСН</br>
		/// </summary>
		/// <param name="idDirection"></param>
		/// <returns><c>true</c>, если все данные найдены,
		/// <br><c>false</c> - в обратном случае или если нет ID </br></returns>
		public bool SetDirection(int? idDirection)
		{
			int? idDepartment = null;
			bool nulify()
			{
				IdНаправления = null;
				// Обнуляем остальные связанные данные вверх по иерархии учреждения
				// Направление -> кафедра
				return (SetDepartment(idDepartment));
			}

			if (!idDirection.HasValue)
			{
				return nulify();
			}

			Направление? directions = Directions.FirstOrDefault(s => s.IdНаправления == idDirection);
			if (directions != null)
			{
				IdНаправления = directions.IdНаправления;
				idDepartment = Departments
					.FirstOrDefault(d => d.IdКафедры == (Directions
						.FirstOrDefault(d => d.IdНаправления == IdНаправления)
						?.IdКафедры ?? -1))
					?.IdКафедры;

				bool isOk = SetDepartment(idDepartment);
				if (!isOk)
				{
					IdНаправления = null;
				}

				return isOk;
			}

			return nulify();
		}

		/// <summary>
		/// Устанавливает ID кафедры и заполняет на основе его данных данные о работе:
		/// <br>Институт, УГСН и стандарт УГСН</br> 
		/// </summary>
		/// <param name="idDepartment"></param>
		/// <returns><c>true</c>, если все данные найдены,
		/// <br><c>false</c> - в обратном случае или если нет ID </br></returns>
		public bool SetDepartment(int? idDepartment)
		{
			bool nulify()
			{
				IdКафедры = IdУгсн = IdИнститута = IdУгснСтандарта = null;
				// Конец иерархии
				return false;
			}

			if (!idDepartment.HasValue) return nulify();

			Кафедра? department = Departments.FirstOrDefault(s => s.IdКафедры == idDepartment);
			if (department != null)
			{
				IdКафедры = idDepartment;
				Угсн? ugsn = Ugsns.FirstOrDefault(d => d.IdУгсн == (Departments
						.FirstOrDefault(d => d.IdКафедры == IdКафедры)?.IdУгсн ?? -1));
				if (ugsn != null)
				{
					IdУгсн = ugsn.IdУгсн;
					IdУгснСтандарта = ugsn.IdУгснСтандарта;
				}
				IdИнститута = Institutes
					.FirstOrDefault(d => d.IdИнститута == (Departments
						.FirstOrDefault(d => d.IdКафедры == IdКафедры)
						?.IdИнститута ?? -1))
					?.IdИнститута;

				bool isOk = IdИнститута.HasValue;
				if (!isOk)
				{
					IdКафедры = IdУгсн = IdИнститута = IdУгснСтандарта = null;
				}

				return isOk;
			}

			return nulify();
		}
	}
}
