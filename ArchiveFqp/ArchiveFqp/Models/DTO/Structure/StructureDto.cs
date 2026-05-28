using ArchiveFqp.Models.Database;
using UglyToad.PdfPig;

namespace ArchiveFqp.Models.DTO.Structure
{
    /// <summary>
    /// Объект, содержащий названия структур для отображения информации
    /// </summary>
    public class StructureDto : IDisplayDto, IEquatable<StructureDto?>
    {
        public Институт Институт { get; set; } = new();
        public Кафедра Кафедра { get; set; } = new();
        public Угсн Угсн { get; set; } = new();
        public УгснСтандарт УгснСтандарт { get; set; } = new();
        public Направление Направление { get; set; } = new();
        public Профиль? Профиль { get; set; }

        public StructureDto()
        {
        }

        public StructureDto(StructureDto other)
        {
            //Институт = structure.Институт;
            //Кафедра = structure.Кафедра;
            //Угсн = structure.Угсн;
            //УгснСтандарт = structure.УгснСтандарт;
            //Направление = structure.Направление;
            //Профиль = structure.Профиль;
            Copy(other);
        }

        public void Copy(StructureDto other)
        {
            Институт = new()
            {
                IdИнститута = other.Институт.IdИнститута,
                Название = other.Институт.Название
            };
            Кафедра = new()
            {
                IdКафедры = other.Кафедра.IdКафедры,
                IdИнститута = other.Кафедра.IdИнститута,
                Название = other.Кафедра.Название
            };
            Угсн = new()
            {
                IdУгсн = other.Угсн.IdУгсн,
                IdУгснСтандарта = other.Угсн.IdУгснСтандарта,
                Название = other.Угсн.Название
            };
            УгснСтандарт = new()
            {
                IdУгснСтандарта = other.УгснСтандарт.IdУгснСтандарта,
                Название = other.УгснСтандарт.Название
            };
            Направление = new()
            {
                IdНаправления = other.Направление.IdНаправления,
                IdКафедры = other.Направление.IdКафедры,
                IdУгсн = other.Направление.IdУгсн,
                Название = other.Направление.Название
            };
            Профиль = other.Профиль is null ? null : new()
            {
                IdПрофиля = other.Профиль?.IdПрофиля ?? 0,
                IdНаправления = other.Профиль?.IdНаправления ?? 0,
                Название = other.Профиль?.Название ?? ""
            };
        }

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

        public static string Abbreviate(string input, bool withAnd = false)
                => string.Join(string.Empty, input.Split([' ', '-'], StringSplitOptions.RemoveEmptyEntries)
                                // Убираем частицы и, в, во, или если withAnd, то допускаем и
                                .Where(s => (withAnd && s == "и") || s.Length > 2)
                                // Оставляем код направления, если это направление, а в остальных случаях пишем аббревиатуры
                                .Select(s => (s.Length == 8 && s.Contains('.')) ? s + " " : s[..1])).ToUpper();


        /// <summary>
        /// Определяет, принадлежит ли указанная структура данной структуре
        /// </summary>
        /// <param name="other">Структура для проверки принадлежности</param>
        /// <returns></returns>
        public bool IsBelongsStructure(StructureDto other)
        {
            if (Институт.IdИнститута != 0 && other.Институт.IdИнститута != Институт.IdИнститута) return false;
            if (Кафедра.IdКафедры != 0 && other.Кафедра.IdКафедры != Кафедра.IdКафедры) return false;
            if (УгснСтандарт.IdУгснСтандарта != 0 && other.УгснСтандарт.IdУгснСтандарта != УгснСтандарт.IdУгснСтандарта) return false;
            if (Угсн.IdУгсн != 0 && other.Угсн.IdУгсн != Угсн.IdУгсн) return false;
            if (Направление.IdНаправления != 0 && other.Направление.IdНаправления != Направление.IdНаправления) return false;
            if (Профиль is not null && Профиль.IdПрофиля != 0 && other.Профиль?.IdПрофиля != Профиль.IdПрофиля) return false;
            return true;
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as StructureDto);
        }

        public bool Equals(StructureDto? other)
        {
            return other is not null &&
                   EqualityComparer<int>.Default.Equals(Институт.IdИнститута, other.Институт.IdИнститута) &&
                   EqualityComparer<int>.Default.Equals(Кафедра.IdКафедры, other.Кафедра.IdКафедры) &&
                   EqualityComparer<int>.Default.Equals(Угсн.IdУгсн, other.Угсн.IdУгсн) &&
                   EqualityComparer<int>.Default.Equals(УгснСтандарт.IdУгснСтандарта, other.УгснСтандарт.IdУгснСтандарта) &&
                   EqualityComparer<int>.Default.Equals(Направление.IdНаправления, other.Направление.IdНаправления) &&
                   EqualityComparer<int?>.Default.Equals(Профиль?.IdПрофиля, other.Профиль?.IdПрофиля);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Институт.IdИнститута, Кафедра.IdКафедры, 
                Угсн.IdУгсн, УгснСтандарт.IdУгснСтандарта, Направление.IdНаправления, 
                Профиль?.IdПрофиля);
        }
    }
}
