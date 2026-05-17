using ArchiveFqp.Models.Database;
using UglyToad.PdfPig;

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

        /// <summary>
        /// Получение текстовой информации о структуре
        /// </summary>
        /// <remarks>T может быть одним из следующих типов: <see cref="Database.Институт"/>, <see cref="Database.Кафедра"/>, <see cref="Database.Направление"/>, <see cref="Database.Профиль"/></remarks>
        /// <typeparam name="T">1 из <see cref="Database.Институт"/>, <see cref="Database.Кафедра"/>, <see cref="Database.Направление"/>, <see cref="Database.Профиль"/></typeparam>
        /// <returns></returns>
        public string GetStructureInfo<T>() where T : class
        {
            return typeof(T).Name switch
            {
                nameof(Database.Институт) => "",
                nameof(Database.Кафедра) => $"Институт: {Институт.Название}\n",
                nameof(Database.Направление) => $"Кафедра: {Кафедра.Название}\n" +
                                            $"УГСН: {Угсн.Название}\n" +
                                            $"Стандарт УГСН: {УгснСтандарт.Название}\n" +
                                            $"Институт: {Институт.Название}",
                nameof(Database.Профиль) => $"Направление: {Направление.Название}\n" +
                                            $"Кафедра: {Кафедра.Название}\n" +
                                            $"УГСН: {Угсн.Название}\n" +
                                            $"Стандарт УГСН: {УгснСтандарт.Название}\n" +
                                            $"Институт: {Институт.Название}",
                _ => throw new ArgumentException("Класс должен быть 1 из Институт, Кафедра, Направление, Профиль"),
            };
        }
    }
}
