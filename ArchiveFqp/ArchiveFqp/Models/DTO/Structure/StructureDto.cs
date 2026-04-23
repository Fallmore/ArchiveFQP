using ArchiveFqp.Models.Database;

namespace ArchiveFqp.Models.DTO.Structure
{
    /// <summary>
    /// Объект, содержащий названия структур для отображения информации
    /// </summary>
    public class StructureDto : IDisplayDto
    {
        public Институт Институт { get; set; } = new();
        public Кафедра Кафедра { get; set; } = new();
        public Угсн Угсн { get; set; } = new();
        public УгснСтандарт УгснСтандарт { get; set; } = new();
        public Направление Направление { get; set; } = new();
        public Профиль? Профиль { get; set; }
    }
}
