using ArchiveFqp.Models.ReferenceData;

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
        /// <typeparam name="T">Класс таблицы</typeparam>
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
        Task<ReferenceDataSnapshot> GetAllAsync(bool forceRefresh = false);

        void ClearCache();

        event EventHandler<List<string>>? TablesChanged;
    }
}
