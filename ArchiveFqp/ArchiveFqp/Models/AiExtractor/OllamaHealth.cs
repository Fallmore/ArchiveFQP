namespace ArchiveFqp.Models.AiExtractor
{
    public class OllamaHealth
    {
        public bool Running { get; set; }
        public List<string> Models { get; set; } = [];
        public string Error { get; set; } = string.Empty;
    }
}
