using ArchiveFqp.Models.Database;

namespace ArchiveFqp.Models.DTO.User
{
    /// <summary>
    /// Объект пользователя <see cref="Database.Пользователь"/> и его аккаунта 
    /// <see cref="АккаунтПользователя"/> для отображения информации
    /// </summary>
    public class UserDisplayDto: IUserDisplayDto, IDisplayDto
    {
        public Пользователь Пользователь { get; set; } = new();

        public List<string> Роли { get; set; } = new();

        public string ФИО => $"{Пользователь.Фамилия} {Пользователь.Имя}{(Пользователь.Отчество != null ? " " + Пользователь.Отчество : "")}";
        public string ФИОИнициалы => $"{Пользователь.Фамилия} {Пользователь.Имя[0]}.{(Пользователь.Отчество != null ? " " + Пользователь.Отчество[0] + "." : "")}";

    }
}
