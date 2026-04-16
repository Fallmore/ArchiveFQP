using ArchiveFqp.Models.Database;
using ArchiveFqp.Models.DTO.Student;
using ArchiveFqp.Models.DTO.Teacher;

namespace ArchiveFqp.Models.Settings.User
{
    public class SettingsUser
    {
        public Пользователь User { get; set; } = null!;
        public АккаунтПользователя Account { get; set; } = null!;

        public StudentDisplayDto? Student { get; set; }
        public bool IsStudent => Student != null;
        public TeacherDisplayDto? Teacher { get; set; }
        public bool IsTeacher => Teacher != null;

    }
}
