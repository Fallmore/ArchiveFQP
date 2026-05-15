using ArchiveFqp.Models.Auth;
using System.ComponentModel.DataAnnotations;

namespace ArchiveFqp.Models.Database;

public partial class АккаунтПользователя
{
    public int IdАккаунта { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Выберите пользователя")]
    public int IdПользователя { get; set; }

    [Required(ErrorMessage = "Придумайте логин")]
    public string Логин { get; set; } = null!;

    [PasswordValidation(8, true, true, true, true)]
    public string Пароль { get; set; } = null!;

    public List<int> Роли { get; set; } = null!;

    public virtual Пользователь IdПользователяNavigation { get; set; } = null!;
}
