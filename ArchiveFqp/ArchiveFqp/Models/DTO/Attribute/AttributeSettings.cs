using System.Text.Json.Serialization;

namespace ArchiveFqp.Models.DTO.Attribute
{
    public class AttributeSettings
    {
        [JsonPropertyName("query")]
        public string Query { get; set; } = string.Empty;
        [JsonPropertyName("examples")]
        public string[]? Examples { get; set; }
        [JsonPropertyName("keywords")]
        public string[]? Keywords { get; set; }
    }
}
