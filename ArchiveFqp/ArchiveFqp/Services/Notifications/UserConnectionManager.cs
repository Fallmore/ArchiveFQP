namespace ArchiveFqp.Services.Notifications
{
    public class UserConnectionManager
    {
        private readonly Dictionary<string, string> _userConnections = [];
        private readonly Lock _lock = new();

        public void AddConnection(string userId, string connectionId)
        {
            lock (_lock)
            {
                _userConnections[connectionId] = userId;
                Console.WriteLine($"[ConnectionManager] Добавлено: User={userId}, Connection={connectionId}");
            }
        }

        public void RemoveConnection(string connectionId)
        {
            lock (_lock)
            {
                if (_userConnections.TryGetValue(connectionId, out var userId))
                {
                    _userConnections.Remove(connectionId);
                    Console.WriteLine($"[ConnectionManager] Удалено: Connection={connectionId}, User={userId}");
                }
            }
        }

        public string? GetUserId(string connectionId)
        {
            lock (_lock)
            {
                return _userConnections.GetValueOrDefault(connectionId);
            }
        }

        public List<string> GetConnectionIds(string userId)
        {
            lock (_lock)
            {
                return _userConnections
                    .Where(x => x.Value == userId)
                    .Select(x => x.Key)
                    .ToList();
            }
        }

        public List<string> GetAllUsers()
        {
            lock (_lock)
            {
                return _userConnections.Values.Distinct().ToList();
            }
        }
    }
}
