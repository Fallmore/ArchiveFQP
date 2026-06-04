namespace ArchiveFqp.Models.FileUpload
{
    /// <summary>
    /// Информация о временно загруженном файле
    /// </summary>
    public class TempFileInfo
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }

        public string SessionId { get; set; } = string.Empty;
        public string TempFilePath { get; set; } = string.Empty;
        public string RelativePath { get; set; } = string.Empty;

        public string OriginalFileName { get; set; } = string.Empty;
        public string StoredFileName { get; set; } = string.Empty;
        public long FileSize { get; set; }

        public string FileTypeKey { get; set; } = string.Empty;
        public string FileTypeName { get; set; } = string.Empty;

        public DateTime UploadedAt { get; set; }

        public string FormattedSize => FormatFileSize(FileSize);

        private static string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }
    }
}
