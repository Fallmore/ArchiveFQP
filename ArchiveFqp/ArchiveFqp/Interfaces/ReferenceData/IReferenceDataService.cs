using ArchiveFqp.Models.DatabaseNotification;
using ArchiveFqp.Models.ReferenceData;
using ArchiveFqp.Models.DTO.Attribute;

namespace ArchiveFqp.Interfaces.ReferenceData
{
    /// <summary>
    /// Обеспечивает механизм взаимодействия с данными таблиц БД через кэш
    /// </summary>
    public interface IReferenceDataService
    {
        /// <summary>
        /// Получение списка данных таблицы
        /// </summary>
        /// <remarks>
        /// T может быть одним из классов пространства <see cref="Models.Database"/>
        /// или класс <see cref="AttributeValuesDto"/>
        /// </remarks>
        /// <typeparam name="T">Класс таблицы или <see cref="AttributeValuesDto"/></typeparam>
        /// <param name="forceRefresh">Принудительно ли обновлять общий кэш</param>
        /// <param name="onlyParentData">Брать ли данные только из родительской таблицы</param>
        /// <returns>Список с элементами типа <typeparamref name="T"/>, если данный тип числится в кэше, 
        /// в ином случае - без элементов</returns>
        Task<List<T>> GetAsync<T>(bool forceRefresh = false, bool onlyParentData = true) where T : class;

        /// <summary>
        /// Получение объекта <see cref="ReferenceDataSnapshot"/>, содержащий частоиспользуемые редкоизменяемые таблицы
        /// </summary>
        /// <param name="forceRefresh"></param>
        /// <returns></returns>
        Task<ReferenceDataSnapshot> GetSnapshotAsync(bool forceRefresh = false);

        void ClearCache();

        event EventHandler<TableChangeEvent>? TablesChanged;
    }
}
