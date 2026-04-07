using ArchiveFqp.Models.DTO;

namespace ArchiveFqp.Factories.DisplayDto
{
    /// <summary>
    /// Обеспечивает механизм создания DTO со строковыми полями для отображения
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="V"></typeparam>
    public interface IDisplayDtoFactory<T, V> where T : IDisplayDto
    {
        /// <summary>
        /// Создает DTO из исходного объекта
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        Task<T> CreateDisplayDtoAsync(V obj);

        /// <summary>
        /// Создает DTO из id объекта
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<T?> CreateDisplayDtoAsync(int id);

        /// <summary>
        /// Создает список DTO из списков объектов
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        Task<List<T>> CreateDisplayDtoListAsync(IEnumerable<V> obj);
    }
}
