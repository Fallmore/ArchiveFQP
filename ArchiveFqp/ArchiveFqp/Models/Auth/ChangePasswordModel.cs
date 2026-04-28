using System.ComponentModel.DataAnnotations;

namespace ArchiveFqp.Models.Auth
{
    public class ChangePasswordModel
    {
        public string Login { get; set; } = string.Empty;
        public string OldPassword { get; set; } = string.Empty;

        [PasswordValidation(8, true, true, true, true)]
        public string NewPassword { get; set; } = string.Empty;

        [Compare("NewPassword", ErrorMessage = "Пароли должны совпадать")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
