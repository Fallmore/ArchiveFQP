using System.ComponentModel.DataAnnotations;

namespace ArchiveFqp.Models.DTO.User
{
    public class UserStructureDto
    {
        public UserType UserType { get; set; } = UserType.Student;

        [Required(ErrorMessage = "Выберите институт")]
        public int? IdInstitute { get; set; }

        [Required(ErrorMessage = "Выберите кафедру")]
        public int? IdDepartment { get; set; }

        public int? IdPost { get; set; }

        public int? IdDirection { get; set; }
        public int? IdProfile { get; set; }
        public int? IdEducationLevel { get; set; }
        public int? IdEducationForm { get; set; }
        public int? YearGraduation { get; set; }
    }
}
