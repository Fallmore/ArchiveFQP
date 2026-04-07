using Newtonsoft.Json.Linq;

namespace ArchiveFqp.Services.DatabaseNotification
{
    /// <summary>
    /// Класс, содержащий информацию об изменении таблицы
    /// </summary>
    public class TableChangeEvent
    {
        public string TableName { get; set; } = "";
        public TableChangeType ChangeType { get; set; }
        public JObject? Old { get; set; }
        public JObject? New { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
