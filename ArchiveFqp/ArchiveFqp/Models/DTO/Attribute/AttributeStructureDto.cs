using ArchiveFqp.Models.Database;
using ArchiveFqp.Models.DTO.Structure;

namespace ArchiveFqp.Models.DTO.Attribute
{
    public class AttributeStructureDto
    {
        public int IdСтруктуры { get; set; }

        public Атрибут Атрибут { get; set; } = null!;

        public СтатусРаботы? СтатусРаботы { get; set; }

        public ТипРаботы? ТипРаботы { get; set; }

        public StructureDto? Структура { get; set; }

        public StructureType ТипСтруктуры { get; set; } = StructureType.Учреждение;

        /// <summary>
        /// Конвертация DTO в структуру атрибута
        /// </summary>
        /// <remarks>T может быть одним из следующих типов: <see cref="АтрибутУчреждения"/>, <see cref="АтрибутИнститута"/>, <see cref="АтрибутКафедры"/>, <see cref="АтрибутНаправления"/>, <see cref="АтрибутПрофиля"/></remarks>
        /// <typeparam name="T">1 из <see cref="АтрибутУчреждения"/>, <see cref="АтрибутИнститута"/>, <see cref="АтрибутКафедры"/>, <see cref="АтрибутНаправления"/>, <see cref="АтрибутПрофиля"/></typeparam>
        /// <returns></returns>
        public T ToAttributeStructure<T>() where T : class
        {
            return typeof(T).Name switch
            {
                nameof(АтрибутУчреждения) => (new АтрибутУчреждения
                {
                    IdСтруктуры = IdСтруктуры,
                    IdАтрибута = Атрибут.IdАтрибута,
                    IdСтатусаРаботы = СтатусРаботы?.IdСтатусаРаботы,
                    IdТипаРаботы = ТипРаботы?.IdТипаРаботы,
                } as T)!,
                nameof(АтрибутИнститута) => (new АтрибутИнститута
                {
                    IdСтруктуры = IdСтруктуры,
                    IdАтрибута = Атрибут.IdАтрибута,
                    IdСтатусаРаботы = СтатусРаботы?.IdСтатусаРаботы,
                    IdТипаРаботы = ТипРаботы?.IdТипаРаботы,
                    IdИнститута = Структура!.Институт.IdИнститута
                } as T)!,
                nameof(АтрибутКафедры) => (new АтрибутКафедры
                {
                    IdСтруктуры = IdСтруктуры,
                    IdАтрибута = Атрибут.IdАтрибута,
                    IdСтатусаРаботы = СтатусРаботы?.IdСтатусаРаботы,
                    IdТипаРаботы = ТипРаботы?.IdТипаРаботы,
                    IdКафедры = Структура!.Кафедра.IdКафедры
                } as T)!,
                nameof(АтрибутНаправления) => (new АтрибутНаправления
                {
                    IdСтруктуры = IdСтруктуры,
                    IdАтрибута = Атрибут.IdАтрибута,
                    IdСтатусаРаботы = СтатусРаботы?.IdСтатусаРаботы,
                    IdТипаРаботы = ТипРаботы?.IdТипаРаботы,
                    IdНаправления = Структура!.Направление.IdНаправления
                } as T)!,
                nameof(АтрибутПрофиля) => (new АтрибутПрофиля
                {
                    IdСтруктуры = IdСтруктуры,
                    IdАтрибута = Атрибут.IdАтрибута,
                    IdСтатусаРаботы = СтатусРаботы?.IdСтатусаРаботы,
                    IdТипаРаботы = ТипРаботы?.IdТипаРаботы,
                    IdПрофиля = Структура!.Профиль!.IdПрофиля
                } as T)!,
                _ => throw new InvalidOperationException($"Неизвестный тип {typeof(T).Name}"),
            };
        }
    }
}
