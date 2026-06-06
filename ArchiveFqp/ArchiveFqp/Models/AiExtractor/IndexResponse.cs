using System.Text.Json.Serialization;

namespace ArchiveFqp.Models.AiExtractor
{
    public class IndexResponse
    {
        [JsonPropertyName("file_path")]
        public string FilePath { get; set; } = string.Empty;
        public bool Indexed { get; set; }
        [JsonPropertyName("total_chunks")]
        public int TotalChunks { get; set; }
        public string Message { get; set; } = string.Empty;
        public string Error { get; set; } = string.Empty;
    }
}
