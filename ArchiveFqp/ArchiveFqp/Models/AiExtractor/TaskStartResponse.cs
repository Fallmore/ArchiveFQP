using System.Text.Json.Serialization;

namespace ArchiveFqp.Models.AiExtractor
{
    public class TaskStartResponse
    {
        [JsonPropertyName("task_id")]
        public string TaskId { get; set; } = string.Empty;
        /// <summary>
        /// "running", "error"
        /// </summary>
        public string Status { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Error { get; set; } = string.Empty;
    }
}
