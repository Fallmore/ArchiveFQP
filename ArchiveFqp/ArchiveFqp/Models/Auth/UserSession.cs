namespace ArchiveFqp.Models.Auth
{
    public class UserSession
    {
        public int UserId { get; set; }
        public string ФИО { get; set; } = string.Empty;
        public string Логин { get; set; } = string.Empty;
        public List<string> Роли { get; set; } = new();
    }
}
