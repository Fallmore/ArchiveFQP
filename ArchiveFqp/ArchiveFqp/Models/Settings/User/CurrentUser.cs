using ArchiveFqp.Interfaces.User;
using ArchiveFqp.Models.Database;
using ArchiveFqp.Models.DTO.Student;
using ArchiveFqp.Models.DTO.Teacher;
using ArchiveFqp.Models.DTO.User;

namespace ArchiveFqp.Models.Settings.User
{
    public class SettingsUser
    {
        public Пользователь? User { get; set; } = null!;
        public АккаунтПользователя? Account { get; set; } = null!;

        public UserDisplayDto? UserDisplay { get; set; }
        public StudentDisplayDto? Student { get; set; }
        public bool IsStudent => Student != null;
        public TeacherDisplayDto? Teacher { get; set; }
        public bool IsTeacher => Teacher != null;

        public SettingsUser(IUserService userService, int idUser)
        {
            _ = Initialize(userService, idUser);
        }

        private async Task Initialize(IUserService userService, int idUser)
        {
            User = await userService.GetUserAsync(idUser);
            if (User != null)
            {
                UserDisplay = await userService.GetUserDisplayAsync(User);
                Account = await userService.GetUserAccountAsync(User);
                Student = await userService.GetStudentDisplayAsync(User);
                Teacher = await userService.GetTeacherDisplayAsync(User);
            }
        }
    }
}
