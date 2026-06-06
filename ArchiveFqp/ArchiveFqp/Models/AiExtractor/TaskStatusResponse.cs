using System.Text.Json;
using System.Text.Json.Serialization;

namespace ArchiveFqp.Models.AiExtractor
{
    public class TaskStatusResponse
    {
        [JsonPropertyName("task_id")]
        public string TaskId { get; set; } = string.Empty;
        /// <summary>
        /// "running", "completed", "cancelled", "error"
        /// </summary>
        public string Status { get; set; } = string.Empty;
        public JsonElement? Result { get; set; }
        public string Error { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}
