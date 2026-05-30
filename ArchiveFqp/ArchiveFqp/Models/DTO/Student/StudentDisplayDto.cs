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
        public int IdСтудента { get; set; }

        public UserDisplayDto Пользователь { get; set; } = new();

        public StructureDto Структура { get; set; } = new();

        public УровеньОбразования УровеньОбразования { get; set; } = new();
        public ФормаОбучения ФормаОбучения { get; set; } = new();
        public int ГодОкончания { get; set; }
        public bool Активно { get; set; }

        public Студент ToStudent()
        {
            return new Студент
            {
                IdСтудента = IdСтудента,
                IdПользователя = Пользователь.Пользователь.IdПользователя,
                IdИнститута = Структура.Институт.IdИнститута,
                IdКафедры = Структура.Кафедра.IdКафедры,
                IdНаправления = Структура.Направление.IdНаправления,
                IdПрофиля = Структура.Профиль?.IdПрофиля,
                IdУровняОбразования = УровеньОбразования.IdУровняОбразования,
                IdФормыОбучения = ФормаОбучения.IdФормыОбучения,
                ГодОкончания = ГодОкончания
            };
        }
    }
}
