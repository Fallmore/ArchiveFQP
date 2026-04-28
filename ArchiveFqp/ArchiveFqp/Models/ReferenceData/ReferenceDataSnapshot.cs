using ArchiveFqp.Models.Database;
using Microsoft.EntityFrameworkCore;

namespace ArchiveFqp.Models.ReferenceData
{
    /// <summary>
    /// Класс со списками частоиспользуемых редкоизменяемых таблиц/справочников БД
    /// </summary>
    public class ReferenceDataSnapshot
    {
        public List<Атрибут> Attributes { get; set; } = [];
        public List<АтрибутУчреждения> AttributesOrganization { get; set; } = [];
        public List<АтрибутИнститута> AttributesInstitute { get; set; } = [];
        public List<АтрибутКафедры> AttributesDepartment { get; set; } = [];
        public List<АтрибутНаправления> AttributesDirection { get; set; } = [];
        public List<АтрибутПрофиля> AttributesProfile { get; set; } = [];
        public List<Должность> Posts { get; set; } = [];
        public List<ДоступРаботы> WorkAccess { get; set; } = [];
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

        public static List<string> GetStaticTableNames()
        {
            return typeof(ReferenceDataSnapshot)
                .GetProperties()
                .Where(p => p.PropertyType.IsGenericType &&
                           p.PropertyType.GetGenericTypeDefinition() == typeof(List<>))
                .Select(p => p.PropertyType.GenericTypeArguments[0].Name)
                .ToList();
        }

        // Не забудьте обновлять этот метод при добавлении новых таблиц в ReferenceDataSnapshot,
        // а также в ReferenceDataService.cs:RefreshReferenceDataSnapshot()
        public List<T> GetTable<T>() where T : class
        {
            return typeof(T).Name switch
            {
                nameof(Атрибут) => (Attributes as List<T>)!,
                nameof(АтрибутУчреждения) => (AttributesOrganization as List<T>)!,
                nameof(АтрибутИнститута) => (AttributesInstitute as List<T>)!,
                nameof(АтрибутКафедры) => (AttributesDepartment as List<T>)!,
                nameof(АтрибутНаправления) => (AttributesDirection as List<T>)!,
                nameof(АтрибутПрофиля) => (AttributesProfile as List<T>)!,
                nameof(Должность) => (Posts as List<T>)!,
                nameof(ДоступРаботы) => (WorkAccess as List<T>)!,
                nameof(Институт) => (Institutes as List<T>)!,
                nameof(Кафедра) => (Departments as List<T>)!,
                nameof(Консультант) => (Consultants as List<T>)!,
                nameof(Направление) => (Directions as List<T>)!,
                nameof(Преподаватель) => (Teachers as List<T>)!,
                nameof(Профиль) => (Profiles as List<T>)!,
                nameof(Рецензент) => (Reviewers as List<T>)!,
                nameof(РольПользователя) => (RoleUsers as List<T>)!,
                nameof(СтатусРаботы) => (WorkStatuses as List<T>)!,
                nameof(Студент) => (Students as List<T>)!,
                nameof(ТипРаботы) => (WorkTypes as List<T>)!,
                nameof(Угсн) => (Ugsns as List<T>)!,
                nameof(УгснСтандарт) => (UgsnStandarts as List<T>)!,
                nameof(УровеньОбразования) => (EducationLevels as List<T>)!,
                nameof(ФормаОбучения) => (EducationForms as List<T>)!,
                _ => throw new ArgumentException($"Неизвестная таблица: {typeof(T).Name}")
            };
        }

        // Не забудьте обновлять этот метод при добавлении новых таблиц в ReferenceDataSnapshot
        // а также в ReferenceDataService.cs:RefreshReferenceDataSnapshot()
        public void SetTable<T>(List<T> table) where T : class
        {
            switch (typeof(T).Name)
            {
                case nameof(Атрибут): Attributes = (table as List<Атрибут>)!; break;
                case nameof(АтрибутУчреждения): AttributesOrganization = (table as List<АтрибутУчреждения>)!; break;
                case nameof(АтрибутИнститута): AttributesInstitute = (table as List<АтрибутИнститута>)!; break;
                case nameof(АтрибутКафедры): AttributesDepartment = (table as List<АтрибутКафедры>)!; break;
                case nameof(АтрибутНаправления): AttributesDirection = (table as List<АтрибутНаправления>)!; break;
                case nameof(АтрибутПрофиля): AttributesProfile = (table as List<АтрибутПрофиля>)!; break;
                case nameof(Должность): Posts = (table as List<Должность>)!; break;
                case nameof(ДоступРаботы): WorkAccess = (table as List<ДоступРаботы>)!; break;
                case nameof(Институт): Institutes = (table as List<Институт>)!; break;
                case nameof(Кафедра): Departments = (table as List<Кафедра>)!; break;
                case nameof(Консультант): Consultants = (table as List<Консультант>)!; break;
                case nameof(Направление): Directions = (table as List<Направление>)!; break;
                case nameof(Преподаватель): Teachers = (table as List<Преподаватель>)!; break;
                case nameof(Профиль): Profiles = (table as List<Профиль>)!; break;
                case nameof(Рецензент): Reviewers = (table as List<Рецензент>)!; break;
                case nameof(РольПользователя): RoleUsers = (table as List<РольПользователя>)!; break;
                case nameof(СтатусРаботы): WorkStatuses = (table as List<СтатусРаботы>)!; break;
                case nameof(Студент): Students = (table as List<Студент>)!; break;
                case nameof(ТипРаботы): WorkTypes = (table as List<ТипРаботы>)!; break;
                case nameof(Угсн): Ugsns = (table as List<Угсн>)!; break;
                case nameof(УгснСтандарт): UgsnStandarts = (table as List<УгснСтандарт>)!; break;
                case nameof(УровеньОбразования): EducationLevels = (table as List<УровеньОбразования>)!; break;
                case nameof(ФормаОбучения): EducationForms = (table as List<ФормаОбучения>)!; break;
                default: throw new ArgumentException($"Unknown type: {typeof(T).Name}");
            };
        }
    }
}
