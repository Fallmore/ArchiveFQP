using ArchiveFqp.Models.Database;

namespace ArchiveFqp.Models.DTO.Student
{
    public class StudentDisplayDto : IUserDisplayDto, IDisplayDto
    {
        public Пользователь Пользователь { get; set; } = new();

        public string Институт { get; set; } = "";
        public string Кафедра { get; set; } = "";
        public string Угсн { get; set; } = "";
        public string УгснСтандарт { get; set; } = "";
        public string Направление { get; set; } = "";
        public string? Профиль { get; set; } = "";

        public string УровеньОбразования { get; set; } = "";
        public string ФормаОбучения { get; set; } = "";
        public int ГодОкончания { get; set; }

        public string ФИО => $"{Пользователь.Фамилия} {Пользователь.Имя}{(Пользователь.Отчество != null ? " " + Пользователь.Отчество : "")}";
        public string ФИОИнициалы => $"{Пользователь.Фамилия} {Пользователь.Имя[0]}.{(Пользователь.Отчество != null ? " " + Пользователь.Отчество[0] + "." : "")}";
    }
}
