using ArchiveFqp.Models.Database;

namespace ArchiveFqp.Models.DTO.Teacher
{
    public class TeacherDisplayDto : IUserDisplayDto, IDisplayDto
    {
        public Пользователь Пользователь { get; set; } = new();

        public string Должность { get; set; } = "";

        public string Институт { get; set; } = "";
        public string Кафедра { get; set; } = "";

        public string ФИО => $"{Пользователь.Фамилия} {Пользователь.Имя}{(Пользователь.Отчество != null ? " " + Пользователь.Отчество : "")}";
        public string ФИОИнициалы => $"{Пользователь.Фамилия} {Пользователь.Имя[0]}.{(Пользователь.Отчество != null ? " " + Пользователь.Отчество[0] + "." : "")}";
    }
}
