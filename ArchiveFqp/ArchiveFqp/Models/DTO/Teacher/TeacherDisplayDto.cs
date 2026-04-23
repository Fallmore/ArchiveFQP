using ArchiveFqp.Models.Database;
using ArchiveFqp.Models.DTO.Structure;
using ArchiveFqp.Models.DTO.User;

namespace ArchiveFqp.Models.DTO.Teacher
{
    /// <summary>
    /// Объект преподавателя <see cref="Преподаватель"/> для отображения информации
    /// </summary>
    public class TeacherDisplayDto: IDisplayDto
    {
        public UserDisplayDto Пользователь { get; set; } = new();

        public string Должность { get; set; } = "";

        public StructureDto Структура { get; set; } = new();
    }
}
