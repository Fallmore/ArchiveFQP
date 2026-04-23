using ArchiveFqp.Models.DTO.Attribute;
using ArchiveFqp.Models.DTO.Student;
using ArchiveFqp.Models.DTO.Teacher;
using ArchiveFqp.Models.DTO.Work;
using System.ComponentModel.DataAnnotations;

namespace ArchiveFqp.Models.DTO.WorkApplication
{
    public class WorkApplicationDto : IDisplayDto
    {
        public int IdЗаявления { get; set; }

        [Required(ErrorMessage = "Выберите работу в каталоге работ")]
        public int? IdРаботы { get; set; }

        public WorkDisplayDto Работа { get; set; } = new();

        public int IdПользователя { get; set; }

        public TeacherDisplayDto? ПользовательПреподаватель { get; set; } = new();
        public StudentDisplayDto? ПользовательСтудент { get; set; } = new();

        [Required(ErrorMessage = "Укажите цель")]
        [MinLength(5, ErrorMessage = "Распишите цель")]
        public string? Цель { get; set; } = "";

        public int IdСтатуса { get; set; }

        public string Статус { get; set; } = "";

        public DateTime ДатаПоступления { get; set; }

        [Required(ErrorMessage = "Укажите дату окончания просмотра работы")]
        public DateTime? ДатаВозврПоЗаявл { get; set; }

        public string? Ответ { get; set; }

        public DateTime? ДатаОтвета { get; set; }

        public DateTime? ДатаВозврПоФакту { get; set; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object? obj)
        {
            WorkApplicationDto? attribute = obj as WorkApplicationDto;
            return Equals(attribute);
        }

        public bool Equals(WorkApplicationDto? other)
        {
            if (other == default) return false;

            if (other.IdЗаявления == IdЗаявления && other.IdРаботы == IdРаботы &&
                other.IdСтатуса == IdСтатуса && other.IdПользователя == IdПользователя &&
                other.ДатаВозврПоЗаявл == ДатаВозврПоЗаявл && other.ДатаВозврПоФакту == ДатаВозврПоФакту &&
                other.Ответ == Ответ && other.ДатаОтвета == ДатаОтвета && other.Цель == Цель &&
                other.ДатаПоступления == ДатаПоступления) return true;
            return false;
        }

        public override int GetHashCode()
        {
            return IdЗаявления.GetHashCode() + IdРаботы.GetHashCode() +
                IdСтатуса.GetHashCode() + IdПользователя.GetHashCode() +
                ДатаВозврПоЗаявл.GetHashCode() + ДатаВозврПоФакту.GetHashCode() +
                (Ответ?.GetHashCode() ?? 0) + (ДатаОтвета?.GetHashCode() ?? 0) + 
                (Цель?.GetHashCode() ?? 0) + ДатаПоступления.GetHashCode();
        }
    }
}
