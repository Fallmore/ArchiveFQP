using ArchiveFqp.Hubs;
using ArchiveFqp.Interfaces.Settings;
using ArchiveFqp.Models.Database;
using ArchiveFqp.Models.Settings.SettingsArchive;
using ArchiveFqp.Models.Settings.User;
using ArchiveFqp.Services.Settings;
using DocumentFormat.OpenXml.Office2010.Excel;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace ArchiveFqp.Services.Notifications
{
    public class NotificationService
    {
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly UserConnectionManager _connectionManager;
        private readonly SettingsArchive _settingsArchive;
        private readonly ISettingsService _settingsService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(IHubContext<NotificationHub> hubContext,
            UserConnectionManager connectionManager, SettingsArchive settingsArchive,
            ISettingsService settingsService, IHttpContextAccessor httpContextAccessor,
            ILogger<NotificationService> logger)
        {
            _hubContext = hubContext;
            _connectionManager = connectionManager;
            _settingsArchive = settingsArchive;
            _settingsService = settingsService;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        // Отправка уведомления конкретному пользователю по ID
        public async Task SendToUserAsync(string userId, string title, string message, string? url = null, bool onlyEmail = false)
        {
            var connectionIds = _connectionManager.GetConnectionIds(userId);

            if (connectionIds.Count != 0 && !onlyEmail && _settingsArchive.SendNotificationsOnEmail)
            {
                foreach (var connectionId in connectionIds)
                {
                    // Отправляем конкретному connectionId
                    await _hubContext.Clients.Client(connectionId).SendAsync("ReceiveNotification", title, message, url);
                }
                //_logger.LogInformation($"Уведомление отправлено пользователю {userId} через {connectionIds.Count} соединений");
            }
            else // Отправка письма, если пользователь не на сайте
            {
                НастройкиПользователя? userSettings = await _settingsService.GetSettings<НастройкиПользователя>(int.Parse(userId));
                if (userSettings != null && _settingsArchive.SendNotifications)
                {
                    SettingsUser? settings = await _settingsService.GetSettingsJson<SettingsUser>(userSettings?.Настройки ?? "");
                    if (settings != null)
                    {
                        if (settings.ReceiveEmailNotifications)
                        {
                            //TODO отправка письма на почту
                        }
                    }
                    else
                    {
                        //TODO отправка письма на почту
                    }
                }
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
