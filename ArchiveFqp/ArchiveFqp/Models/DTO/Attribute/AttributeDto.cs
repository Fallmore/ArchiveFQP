using ArchiveFqp.Models.Database;
using System.ComponentModel.DataAnnotations;

namespace ArchiveFqp.Models.DTO.Attribute
{
    public class AttributeDto : IEquatable<AttributeDto>
    {
        public int IdАтрибута { get; set; } = 0;

        public int IdДанных { get; set; } = 0;

        public int IdСтруктуры { get; set; } = 0;

        public int IdРаботы { get; set; } = 0;

        public string Название { get; set; } = "";

        [Required(ErrorMessage = "Заполните данные")]
        public string Данные { get; set; } = "";

        public AttributeDto(int idАтрибута, int idДанных, int idСтруктуры, int idРаботы, string название, string данные)
        {
            IdАтрибута = idАтрибута;
            IdДанных = idДанных;
            IdСтруктуры = idСтруктуры;
            IdРаботы = idРаботы;
            Название = название;
            Данные = данные;
        }

        /// <summary>
        /// Преобразование ДТО в объект атрибута
        /// </summary>
        /// <returns></returns>
        public Атрибут ToAttribute()
        {
            return new() { IdАтрибута = IdАтрибута, Название = Название };
        }

        /// <summary>
        /// Преобразование ДТО в объект данных по атрибутам
        /// </summary>
        /// <returns></returns>
        public ДанныеПоАтриб ToAttributeValue()
        {
            return new() { IdДанных = IdДанных, IdРаботы = IdРаботы, 
                IdСтруктуры = IdСтруктуры, Данные = Данные};
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object? obj)
        {
            AttributeDto? attribute = obj as AttributeDto;
            return Equals(attribute);
        }

        public bool Equals(AttributeDto? other)
        {
            if (other == default) return false;

            if (other.IdДанных == IdДанных && other.IdАтрибута == IdАтрибута &&
                other.IdСтруктуры == IdСтруктуры && other.IdРаботы == IdРаботы &&
                other.Название == Название) return true;
            return false;
        }

        public override int GetHashCode()
        {
            return IdАтрибута.GetHashCode() + IdДанных.GetHashCode() +
                IdСтруктуры.GetHashCode() + IdРаботы.GetHashCode() +
                Название.GetHashCode();
        }
    }

}
