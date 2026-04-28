using ArchiveFqp.Interfaces.DatabaseNotification;
using ArchiveFqp.Models.DatabaseNotification;
using Newtonsoft.Json;
using Npgsql;
using System.Collections.Concurrent;
using System.Threading.Channels;

namespace ArchiveFqp.Services.DatabaseNotification
{
    /// <summary>
    /// Класс, отвечающий за подписки на уведомления изменений таблиц БД
    /// </summary>
    public class DatabaseNotificationService : IDatabaseNotificationService, IDisposable
    {
        private readonly NpgsqlConnection _connection;
        private readonly ConcurrentDictionary<string, List<Action<TableChangeEvent>>> _handlers = [];
        private readonly ILogger<DatabaseNotificationService> _logger;
        private CancellationTokenSource _cts;
        private Task? _listenerTask;
        private readonly SemaphoreSlim _subscriptionLock = new(1, 1);
        /// <summary> Приписка к названию таблицы </summary>
        /// Если захотите поменять, то надо менять и в триггере в БД
        private readonly static string s_suffix = "_updates";

        public bool IsConnected => _connection.State == System.Data.ConnectionState.Open;

        public DatabaseNotificationService(IConfiguration configuration, ILogger<DatabaseNotificationService> logger)
        {
            string? connectionString = configuration.GetConnectionString("ArchiveFqpContext");
            NpgsqlConnectionStringBuilder builder = new(connectionString)
            {
                Pooling = false,
                KeepAlive = 30,
                Timeout = 30,
                CommandTimeout = 30
            };

            _connection = new NpgsqlConnection(builder.ConnectionString);
            _connection.Open();
            _connection.Notification += OnNotification;
            _logger = logger;
            _cts = new CancellationTokenSource();
        }

        private async Task ListenForNotifications()
        {
            _logger.LogInformation("Слушатель уведомлений включен");

            while (!_cts.Token.IsCancellationRequested)
            {
                try
                {
                    // Ожидаем уведомления с таймаутом для проверки отмены
                    await _connection.WaitAsync(_cts.Token).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("Слушатель уведомлений отменен");
                    break;
                }
                catch (NpgsqlException ex)
                {
                    _logger.LogError(ex, "PostgreSQL-ошибка в слушателе уведомлений");
                    await TryReconnect();
                    await Task.Delay(2000, _cts.Token);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Непредвиденная ошибка в слушателе уведомлений");
                    await TryReconnect();
                    await Task.Delay(2000, _cts.Token);
                }
            }

            _logger.LogInformation("Слушатель уведомлений остановлен");
        }

        private async Task TryReconnect()
        {
            while (!IsConnected && !_cts.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation("Соединение потеряно. Попытка подключения к PostgreSQL...");
                    await _connection.OpenAsync();
                    await ResubscribeToAllChannels();
                    if (IsConnected) _logger.LogInformation("Соединение к PostgreSQL восстановлено");

                }
                catch (Exception reconnectEx)
                {
                    _logger.LogError(reconnectEx, "Ошибка переподключения к PostgreSQL");
                    await Task.Delay(5000, _cts.Token);
                }
            }
        }

        private void StopListenForNotifications()
        {
            _cts?.Cancel();
            try
            {
                _listenerTask?.Wait();
            }
            catch (Exception)
            {

            }
            _cts = new CancellationTokenSource();
        }

        public async Task SubscribeAsync(string tableName, Action<TableChangeEvent> handler)
        {
            await SubscribeAsync([tableName], handler);
        }

        public async Task SubscribeAsync(List<string> tableNames, Action<TableChangeEvent> handler)
        {
            await _subscriptionLock.WaitAsync();
            try
            {
                foreach (string tableName in tableNames)
                {
                    string channel = string.Concat(tableName, s_suffix);

                    // Проверяем, подписан ли уже кто-то на этот канал
                    if (!_handlers.TryGetValue(channel, out List<Action<TableChangeEvent>>? value))
                    {
                        value = [];
                        _handlers[channel] = value;

                        // Запрос к БД не сработает, пока прослушиваются уведомления
                        if (_listenerTask != null) StopListenForNotifications();

                        using NpgsqlCommand cmd = new($"LISTEN {channel}", _connection);
                        await cmd.ExecuteNonQueryAsync();

                        _logger.LogInformation("Подписка на канал: {Channel}", channel);

                        value.Add(handler);
                    }
                }
            }
            finally
            {
                _subscriptionLock.Release();
            }

            EnsureListenerRunning();
        }

        private async Task ResubscribeToAllChannels()
        {
            // Запрос к БД *сработает*, если эту функция вызвана ошибкой,
            // потому что соединение в это время не прослушивается
            //if (_listenerTask != null) StopListenForNotifications();

            foreach (string channel in _handlers.Keys)
            {
                try
                {
                    using NpgsqlCommand cmd = new($"LISTEN {channel}", _connection);
                    await cmd.ExecuteNonQueryAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка при переподписке на канал: {Channel}", channel);
                }
            }

            EnsureListenerRunning();
        }

        private void EnsureListenerRunning()
        {
            if (_listenerTask == null || _listenerTask.IsCompleted)
            {
                _cts?.Cancel();
                _cts = new CancellationTokenSource();
                _listenerTask = Task.Run(ListenForNotifications, _cts.Token);
            }
        }

        public async Task UnsubscribeAsync(string tableName)
        {
            await _subscriptionLock.WaitAsync();
            try
            {
                string channel = string.Concat(tableName, s_suffix);

                if (_handlers.TryRemove(channel, out var handlers))
                {
                    handlers.Clear();

                    // Запрос к БД не сработает, пока прослушиваются уведомления
                    if (_listenerTask != null) StopListenForNotifications();

                    using NpgsqlCommand cmd = new($"UNLISTEN {channel}", _connection);
                    await cmd.ExecuteNonQueryAsync();

                    _logger.LogInformation("Отписка от канала: {Channel}", channel);
                }
            }
            finally
            {
                _subscriptionLock.Release();
            }

            EnsureListenerRunning();
        }

        private void OnNotification(object sender, NpgsqlNotificationEventArgs e)
        {
            _logger.LogDebug("[{Time}] Получено уведомление: Channel={Channel}, Payload={Payload}",
                DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss"), e.Channel, e.Payload);

            if (!_handlers.TryGetValue(e.Channel, out List<Action<TableChangeEvent>>? handlers))
            {
                _logger.LogDebug("Нет обработчиков для канала: {Channel}", e.Channel);
                return;
            }

            try
            {
                TableChangeEvent? changeEvent = JsonConvert.DeserializeObject<TableChangeEvent>(e.Payload);
                if (changeEvent == null) return;

                changeEvent.TableName = e.Channel.Replace(s_suffix, "");

                // Вызываем всех подписчиков
                foreach (Action<TableChangeEvent> handler in handlers)
                {
                    try
                    {
                        handler(changeEvent);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Ошибка в обработчике канала: {Channel}", e.Channel);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка обработки уведомления канала: {Channel}", e.Channel);
            }
        }

        public void Dispose()
        {
            StopListenForNotifications();
            _cts?.Dispose();
            _connection?.Close();
            _connection?.Dispose();

            _logger.LogInformation("DatabaseNotificationService disposed");
        }
    }
}
