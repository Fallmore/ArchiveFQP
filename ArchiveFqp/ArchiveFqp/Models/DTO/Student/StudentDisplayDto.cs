using ArchiveFqp.Models.Database;
using ArchiveFqp.Models.DTO.Structure;
using ArchiveFqp.Models.DTO.User;

namespace ArchiveFqp.Models.DTO.Student
{
    /// <summary>
    /// Объект студента <see cref="Студент"/> для отображения информации
    /// </summary>
    public class StudentDisplayDto : IDisplayDto
    {
        public UserDisplayDto Пользователь { get; set; } = new();

        public StructureDto Структура { get; set; } = new();

        public string УровеньОбразования { get; set; } = "";
        public string ФормаОбучения { get; set; } = "";
        public int ГодОкончания { get; set; }
    }
}
