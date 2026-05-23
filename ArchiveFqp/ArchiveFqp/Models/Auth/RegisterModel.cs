using System.ComponentModel.DataAnnotations;

namespace ArchiveFqp.Models.Auth
{
    public class RegisterModel
    {
        [Required(ErrorMessage = "Введите свою фамилию")]
        public string? Surname { get; set; }

        [Required(ErrorMessage = "Введите свое имя")]
        public string? Name { get; set; }

        public string? Patronymic { get; set; }
        [EmailAddress(ErrorMessage = "неверный формат почты")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Придумайте логин")]
        public string Login { get; set; } = string.Empty;

        [PasswordValidation(8, true, true, true, true)]
        public string Password { get; set; } = string.Empty;

        [Compare($"{nameof(Password)}", ErrorMessage = "Пароли должны совпадать")]
        public string ConfirmPassword { get; set; } = string.Empty;



        public bool IsStudent { get; set; } = true;

        [Required(ErrorMessage = "Выберите институт")]
        public int? IdInstitute { get; set; }

        [Required(ErrorMessage = "Выберите кафедру")]
        public int? IdDepartment { get; set; }

        public int? IdDirection { get; set; }
        public int? IdProfile { get; set; }
    }
}
