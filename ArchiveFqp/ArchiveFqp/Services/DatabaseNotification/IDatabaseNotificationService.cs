namespace ArchiveFqp.Services.DatabaseNotification
{
    /// <summary>
    /// Обеспечивает механизм подписок на уведомления от БД
    /// </summary>
    public interface IDatabaseNotificationService : IDisposable
    {
        /// <summary>
        /// Подписывает на уведомления изменений таблиц БД
        /// </summary>
        /// <param name="tableName">Название таблицы в БД</param>
        /// <param name="handler">Действие, которое выполнится при поступлении уведомления</param>
        /// <returns></returns>
        Task SubscribeAsync(string tableNames, Action<TableChangeEvent> handler);

        /// <summary>
        /// Подписывает на уведомления изменений таблиц БД
        /// </summary>
        /// <param name="tableNames">Список названий таблиц в БД</param>
        /// <param name="handler">Действие, которое выполнится при поступлении уведомления</param>
        /// <returns></returns>
        Task SubscribeAsync(List<string> tableNames, Action<TableChangeEvent> handler);

        /// <summary>
        /// Отписывает от уведомлений изменений таблиц БД
        /// </summary>
        /// <param name="tableName">Название таблицы в БД</param>
        /// <returns></returns>
        Task UnsubscribeAsync(string tableName);
    }
}
