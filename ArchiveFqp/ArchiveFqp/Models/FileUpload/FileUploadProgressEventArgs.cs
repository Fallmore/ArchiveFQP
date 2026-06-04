namespace ArchiveFqp.Models.FileUpload
{
    /// <summary>
    /// Класс для отслеживания прогресса загрузки файла на сервер
    /// </summary>
    public class FileUploadProgressEventArgs : EventArgs
    {
        public string FileName { get; set; } = string.Empty;
        public decimal ProgressPercent { get; set; }
        public long BytesUploaded { get; set; }
        public long TotalBytes { get; set; }
        public string FileType { get; set; } = string.Empty;
        public bool IsTemp { get; set; } = false;
    }
}
