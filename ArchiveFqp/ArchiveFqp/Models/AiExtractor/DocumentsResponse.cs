namespace ArchiveFqp.Models.AiExtractor
{
    public class DocumentsResponse
    {
        public List<string> Documents { get; set; } = [];
        public string Error { get; set; } = string.Empty;
    }
}
