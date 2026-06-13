using ArchiveFqp.Models.Database;
using ArchiveFqp.Models.DTO.Structure;
using ArchiveFqp.Models.DTO.User;

namespace ArchiveFqp.Models.DTO.Teacher
{
    /// <summary>
    /// Объект преподавателя <see cref="Преподаватель"/> для отображения информации
    /// </summary>
    public class TeacherDisplayDto : IDisplayDto
    {
        public int IdПреподавателя { get; set; }

        public UserDisplayDto Пользователь { get; set; } = new();

        public Должность Должность { get; set; } = new();

        public StructureDto Структура { get; set; } = new();

        public List<РольУчреждения> Роли { get; set; } = [];

        public bool Активно { get; set; }

        public Преподаватель ToTeacher()
        {
            return new Преподаватель
            {
                IdПользователя = Пользователь.Пользователь.IdПользователя,
                IdПреподавателя = IdПреподавателя,
                IdДолжности = Должность.IdДолжности,
                IdИнститута = Структура.Институт.IdИнститута,
                IdКафедры = Структура.Кафедра.IdКафедры,
                Роли = [.. Роли.Select(x => x.IdРоли)],
                Активно = Активно
            };
        }
    }
}
