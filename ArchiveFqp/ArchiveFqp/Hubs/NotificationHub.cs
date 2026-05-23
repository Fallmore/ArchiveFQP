using ArchiveFqp.Services.Notifications;
using DocumentFormat.OpenXml.InkML;
using Microsoft.AspNetCore.SignalR;

namespace ArchiveFqp.Hubs
{
    public class NotificationHub : Hub
    {
        private readonly UserConnectionManager _connectionManager;
        private readonly ILogger<NotificationHub> _logger;

        public NotificationHub(UserConnectionManager connectionManager, ILogger<NotificationHub> logger)
        {
            _connectionManager = connectionManager;
            _logger = logger;
        }

        // Метод для привязки текущего пользователя (вызывается из клиента)
        public async Task SetUserId(string userId)
        {
            _connectionManager.AddConnection(userId, Context.ConnectionId);

            //_logger.LogInformation($"User {userId} connected with connection {Context.ConnectionId}");
        }

        public override async Task OnConnectedAsync()
        {
            //_logger.LogInformation($"Новое подключение: {Context.ConnectionId}");
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            _connectionManager.RemoveConnection(Context.ConnectionId);
            //_logger.LogInformation($"Отключение: {Context.ConnectionId}");
            await base.OnDisconnectedAsync(exception);
        }
    }
}
