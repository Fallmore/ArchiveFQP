using ArchiveFqp.Models.Database;
using ArchiveFqp.Models.DTO.Attribute;
using ArchiveFqp.Models.DTO.Student;
using ArchiveFqp.Models.DTO.Teacher;

namespace ArchiveFqp.Models.DTO.Work
{
    /// <summary>
    /// Объект работы <see cref="Работа"/> для отображения информации
    /// </summary>
    public class WorkDisplayDto : IDisplayDto
    {
        public int IdРаботы { get; set; }
        public string Тема { get; set; } = "";

        public StudentDisplayDto Студент { get; set; } = new();
        public TeacherDisplayDto Руководитель { get; set; } = new();
        public List<TeacherDisplayDto>? Консультанты { get; set; }
        public List<TeacherDisplayDto>? Рецензенты { get; set; }

        public ТипРаботы ТипРаботы { get; set; } = new();
        public СтатусРаботы СтатусРаботы { get; set; } = new();
        public ДоступРаботы ДоступРаботы { get; set; } = new();

        public List<AttributeDto>? Атрибуты { get; set; }

        public string? Аннотация { get; set; }
        public int КоличСтраниц { get; set; }
        public DateTime ДатаДобавления { get; set; }
        public DateTime? ДатаИзменения { get; set; }
        public string? Местоположение { get; set; }
        public string? Эцп { get; set; }
    }
}
