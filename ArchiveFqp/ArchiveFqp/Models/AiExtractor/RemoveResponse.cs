using System.Text.Json.Serialization;

namespace ArchiveFqp.Models.AiExtractor
{
    public class RemoveResponse
    {
        [JsonPropertyName("file_path")]
        public string FilePath { get; set; } = string.Empty;
        public bool Removed { get; set; }
        [JsonPropertyName("chunks_removed")]
        public int ChunksRemoved { get; set; }
        [JsonPropertyName("remaining_chunks")]
        public int RemainingChunks { get; set; }
        public string Error { get; set; } = string.Empty;
    }
}
