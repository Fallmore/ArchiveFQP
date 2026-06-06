using System.Text.Json.Serialization;

namespace ArchiveFqp.Models.AiExtractor
{
    public class HealthResponse
    {
        /// <summary>
        /// "ok", "error"
        /// </summary>
        public string Status { get; set; } = string.Empty;
        public OllamaHealth Ollama { get; set; } = new();
        [JsonPropertyName("chroma_db")]
        public bool ChromaDb { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
