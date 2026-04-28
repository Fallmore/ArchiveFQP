namespace ArchiveFqp.Models.FileUpload
{
    /// <summary>
    /// Результат загрузки файла
    /// </summary>
    public class FileUploadResult
    {
        public string OriginalFileName { get; set; } = string.Empty;
        public string StoredFileName { get; set; } = string.Empty;
        public string RelativePath { get; set; } = string.Empty;
        public string RelativeFolderPath => RelativePath[..RelativePath.IndexOf("/" + StoredFileName)];
        public string FullPath { get; set; } = string.Empty;
        public string FullFolderPath => FullPath[..FullPath.IndexOf("\\" + StoredFileName)];
        public long FileSize { get; set; }
        public string ContentType { get; set; } = string.Empty;
        public FileType FileType { get; set; }
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    }
}
