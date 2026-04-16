using ArchiveFqp.Models.DTO.Student;
using ArchiveFqp.Models.DTO.Teacher;
using ArchiveFqp.Models.DTO.Work;
using System.ComponentModel.DataAnnotations;

namespace ArchiveFqp.Models.DTO.WorkApplication
{
    public class WorkApplicationDto : IDisplayDto
    {
        public int IdВыдачи { get; set; }

        [Required(ErrorMessage = "Выберите работу в каталоге работ")]
        public int? IdРаботы { get; set; }

        public WorkDisplayDto Работа { get; set; } = new();

        public int IdПользователя { get; set; }

        public TeacherDisplayDto? ПользовательПреподаватель { get; set; } = new();
        public StudentDisplayDto? ПользовательСтудент { get; set; } = new();

        [Required(ErrorMessage = "Укажите цель")]
        [MinLength(5, ErrorMessage = "Распишите цель")]
        public string? Цель { get; set; } = "";

        public int IdСтатусаВыдачи { get; set; }

        public string СтатусВыдачи { get; set; } = "";

        public DateTime ДатаПоступления { get; set; }

        [Required(ErrorMessage = "Укажите дату окончания просмотра работы")]
        public DateTime? ДатаВозврПоЗаявл { get; set; }

        public string? Ответ { get; set; }

        public DateTime? ДатаОтвета { get; set; }

        public DateTime? ДатаВозврПоФакту { get; set; }
    }
}
