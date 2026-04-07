using ArchiveFqp.Models.Database;
using ArchiveFqp.Models.DTO.Attribute;
using ArchiveFqp.Models.DTO.Work;
using ArchiveFqp.Models.Search;

namespace ArchiveFqp.Services.Work
{
    /// <summary>
    /// Обеспечивает механизм взаимодействия с работами
    /// </summary>
    public interface IWorkService
	{
		/// <summary>
		/// Значения атрибутов, которые не надо выбирать
		/// </summary>
		public readonly static List<string> AbandonedValues = ["Н/Д", "Ожидание поиска..."];

        /// <summary>
        /// Работы, являющиеся дипломными
        /// </summary>
        public readonly static List<string> FqpWorks = ["ВКРБ", "МД"];

        /// <summary>
        /// Работы, имеющие консультантов и рецензентов
        /// </summary>
        public readonly static List<string> FqpWorksWithCR = ["МД"];

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
		/// Получение всех значений каждого атрибута
		/// </summary>
		/// <param name="attrs">Список атрибутов</param>
		/// <returns>Словарь idАтрибута-значение</returns>
		Task<List<AttributeValuesDto>> GetAttributeValuesAsync(List<Атрибут>? attrs = null, List<string>? abandonedValues = null);

        /// <summary>
        /// Получение атрибутов и их значений у работы
        /// </summary>
        /// <param name="idWork"></param>
        /// <param name="abandonedValues">запрещенные значения, которые нужно игнорировать</param>
        /// <returns>Список атрибутDTO.<br></br> В список не будут включены атрибуты, имеющие запрещенные значения</returns>
        Task<List<AttributeDto>?> GetWorkAttributesAsync(int idWork, List<string>? abandonedValues = null);

        /// <summary>
        /// Получение атрибутов и их значений у работ
        /// </summary>
        /// <param name="works"></param>
        /// <returns>Словарь idРаботы-(Список атрибутDTO)</returns>
        Task<Dictionary<int, List<AttributeDto>>> GetWorksAttributesAsync(List<Работа> works, List<string>? abandonedValues = null);

        Task<WorkDisplayDto> GetWorkDisplayAsync(Работа work, List<Консультант>? consultants = null, List<Рецензент>? reviewers = null);

        /// <summary>
        /// В зависимости от типа работы возвращает год
        /// </summary>
        /// <param name="work"></param>
        /// <returns>Если работа относится к ВКР, то год окончания обучения студентом, 
        /// иначе год добавления работы</returns>
        int PickDateWork(Работа work);

        /// <summary>
        /// <inheritdoc cref="PickDateWork"/>
        /// </summary>
        /// <param name="work"></param>
        /// <returns></returns>
        int PickDateWork(WorkDisplayDto work);

        /// <summary>
        /// Устанавливает ID студента и заполняет на основе его данных данные о работе:
        /// <br>Уровень образования, форма обучения</br>
        /// <br>Институт, кафедра, направление, профиль</br>
        /// <br>Год выпуска, УГСН и стандарт УГСН</br> 
        /// </summary>
        /// <param name="idStudent"></param>
        /// <returns><c>true</c>, если все данные найдены,
        /// <br><c>false</c> - в обратном случае или если нет ID </br></returns>
        bool SetStudent(WorkCreateDto work, int? idStudent);

        /// <summary>
        /// Устанавливает ID профиля и заполняет на основе его данных данные о работе:
        /// <br>Институт, кафедра, направление</br> 
        /// <br>УГСН и стандарт УГСН</br>
        /// </summary>
        /// <param name="idProfile"></param>
        /// <returns><c>true</c>, если все данные найдены,
        /// <br><c>false</c> - в обратном случае или если нет ID </br></returns>
        bool SetProfile(WorkCreateDto work, int? idProfile);

        /// <summary>
        /// Устанавливает ID направления и заполняет на основе его данных данные о работе:
        /// <br>Институт, кафедра, УГСН и стандарт УГСН</br>
        /// </summary>
        /// <param name="idDirection"></param>
        /// <returns><c>true</c>, если все данные найдены,
        /// <br><c>false</c> - в обратном случае или если нет ID </br></returns>
        bool SetDirection(WorkCreateDto work, int? idDirection);

        /// <summary>
        /// Устанавливает ID кафедры и заполняет на основе его данных данные о работе:
        /// <br>Институт, УГСН и стандарт УГСН</br> 
        /// </summary>
        /// <param name="idDepartment"></param>
        /// <returns><c>true</c>, если все данные найдены,
        /// <br><c>false</c> - в обратном случае или если нет ID </br></returns>
        bool SetDepartment(WorkCreateDto work, int? idDepartment);
    }

}
