using ArchiveFqp.Models.DTO.Attribute;
using System.Text.Json.Serialization;

namespace ArchiveFqp.Models.AiExtractor
{
    public class ProcessWorkResult
    {
        [JsonPropertyName("work_id")]
        public int WorkId { get; set; }

        [JsonPropertyName("attributes_processed")]
        public int AttributesProcessed { get; set; }

        public List<AttributeDto> Results { get; set; } = new();
    }
}
