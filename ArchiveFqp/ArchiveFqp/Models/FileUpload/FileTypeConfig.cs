namespace ArchiveFqp.Models.FileUpload
{
    /// <summary>
    /// Конфигурация типа файла
    /// </summary>
    public class FileTypeConfig
    {
        public string Key { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public List<string> AllowedExtensions { get; set; } = new();
        public string FilePrefix { get; set; } = string.Empty;
        public bool IsRequired { get; set; } = false;
        public string? Description { get; set; }

        public FileTypeConfig() { }

        public FileTypeConfig(string key, string displayName, List<string> allowedExtensions)
        {
            Key = key;
            DisplayName = displayName;
            AllowedExtensions = allowedExtensions;
        }
    }
}
