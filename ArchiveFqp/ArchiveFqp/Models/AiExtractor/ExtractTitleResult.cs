using System.Text.Json.Serialization;

namespace ArchiveFqp.Models.AiExtractor
{
    public class ExtractTitleResult
    {
        [JsonPropertyName("raw_text")]
        public string RawText { get; set; } = string.Empty;
        public TitleInfo Title { get; set; } = new();
    }
}
