using System.ComponentModel.DataAnnotations;

namespace ArchiveFqp.Models.Auth
{
    public class LoginModel
    {
        [Required(AllowEmptyStrings = false, ErrorMessage ="Введите логин")]
        public string Login { get; set; } = string.Empty;

        [Required(AllowEmptyStrings = false, ErrorMessage ="Введите пароль")]
        public string Password { get; set; } = string.Empty;
    }
}
