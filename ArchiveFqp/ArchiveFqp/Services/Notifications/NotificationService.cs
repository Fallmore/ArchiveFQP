using ArchiveFqp.Hubs;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace ArchiveFqp.Services.Notifications
{
    public class NotificationService
    {
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly UserConnectionManager _connectionManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(
            IHubContext<NotificationHub> hubContext,
            UserConnectionManager connectionManager,
            IHttpContextAccessor httpContextAccessor,
            ILogger<NotificationService> logger)
        {
            _hubContext = hubContext;
            _connectionManager = connectionManager;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        // Отправка уведомления конкретному пользователю по ID
        public async Task SendToUserAsync(string userId, string title, string message, string? url = null)
        {
            var connectionIds = _connectionManager.GetConnectionIds(userId);

            if (connectionIds.Count != 0)
            {
                foreach (var connectionId in connectionIds)
                {
                    // Отправляем конкретному connectionId
                    await _hubContext.Clients.Client(connectionId).SendAsync("ReceiveNotification", title, message, url);
                }
                //_logger.LogInformation($"Уведомление отправлено пользователю {userId} через {connectionIds.Count} соединений");
            }
            else
            {
                _logger.LogWarning("Пользователь {userId} не найден в активных соединениях", userId);
                throw new Exception($"User {userId} is not connected");
            }
        }

        // Отправка уведомления текущему пользователю (из HttpContext)
        public async Task SendToCurrentUserAsync(string title, string message, string? url = null)
        {
            var userId = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                await SendToUserAsync(userId, title, message, url);
            }
            else
            {
                _logger.LogWarning("Не удалось определить текущего пользователя");
            }
        }

        // Отправка всем
        public async Task SendToAllAsync(string title, string message, string? url = null)
        {
            await _hubContext.Clients.All.SendAsync("ReceiveNotification", title, message, url);
        }

        // Отправка уведомления нескольким пользователям
        public async Task SendToUsersAsync(List<string> userIds, string title, string message, string? url = null)
        {
            foreach (var userId in userIds)
            {
                await SendToUserAsync(userId, title, message, url);
            }
        }

        public List<string> GetActiveUsers()
        {
            return _connectionManager.GetAllUsers();
        }
    }
}
