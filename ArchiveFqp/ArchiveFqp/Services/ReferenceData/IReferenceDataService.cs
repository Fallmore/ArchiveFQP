namespace ArchiveFqp.Services.ReferenceData
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
        /// <returns></returns>
        Task<List<T>> GetAsync<T>(bool forceRefresh = false, bool onlyParentData = true) where T : class;
        Task<ReferenceDataSnapshot> GetAllAsync(bool forceRefresh = false);
        void ClearCache();
        
        event EventHandler<List<string>>? TablesChanged;
    }
}
