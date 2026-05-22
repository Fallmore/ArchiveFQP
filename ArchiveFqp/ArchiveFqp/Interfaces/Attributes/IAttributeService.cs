using ArchiveFqp.Models.DTO.Attribute;
using ArchiveFqp.Models.Database;
using ArchiveFqp.Factories;

namespace ArchiveFqp.Interfaces.Attributes
{
    public interface IAttributeService
    {
        /// <inheritdoc cref= "AttributeStructureDtoFactory.CreateAsync{T}(int)" />
        public Task<AttributeStructureDto?> CreateDtoAsync<T>(int idStructure) where T : class;
        /// <inheritdoc cref= "AttributeStructureDtoFactory.CreateAsync{T}(List{int})" />
        public Task<List<AttributeStructureDto>?> CreateDtoAsync<T>(List<int> idStructures) where T : class;

        /// <summary>
        /// Создает DTO <see cref="AttributeStructureDto"/> по атрибуту структуры от типа <typeparamref name="T"/>.
        /// </summary>
        /// <remarks>T может быть одним из следующих типов: <see cref="АтрибутУчреждения"/>, <see cref="АтрибутИнститута"/>, <see cref="АтрибутКафедры"/>, <see cref="АтрибутНаправления"/>, <see cref="АтрибутПрофиля"/></remarks>
        /// <typeparam name="T"><inheritdoc cref= "AttributeStructureDtoFactory.CreateAsync{T}(List{int})" /></typeparam>
        public Task<AttributeStructureDto?> CreateDtoAsync<T>(T structure) where T : class;
        /// <summary>
        /// Создает список DTO <see cref="AttributeStructureDto"/> по атрибуту структуры от типа <typeparamref name="T"/>.
        /// </summary>
        /// <remarks>T может быть одним из следующих типов: <see cref="АтрибутУчреждения"/>, <see cref="АтрибутИнститута"/>, <see cref="АтрибутКафедры"/>, <see cref="АтрибутНаправления"/>, <see cref="АтрибутПрофиля"/></remarks>
        /// <typeparam name="T"><inheritdoc cref= "AttributeStructureDtoFactory.CreateAsync{T}(List{int})" /></typeparam>
        public Task<List<AttributeStructureDto>?> CreateDtoAsync<T>(List<T> structures) where T : class;


        /// <summary> <inheritdoc cref="CrudGeneric.Upsert"/> </summary>
        /// <returns><inheritdoc cref="CrudGeneric.Upsert"/></returns>
        public Task<bool> Upsert<T>(T item) where T : class;
        /// <summary> <inheritdoc cref="CrudGeneric.Delete"/> </summary>
        /// <returns><inheritdoc cref="CrudGeneric.Delete"/></returns>
        public Task<bool> Delete<T>(int id) where T : class;
    }
}
