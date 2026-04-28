using ArchiveFqp.Models.Hash;

namespace ArchiveFqp.Models.FileUpload
{
    /// <summary>
    /// Результат загрузки с хэшами
    /// </summary>
    public class FileUploadWithHashResult
    {
        /// <summary>
        /// <inheritdoc cref="FileUploadResult"/>
        /// </summary>
        public List<FileUploadResult> FileResults { get; set; } = new();
        /// <summary>
        /// <inheritdoc cref="FileHashesInfo"/>
        /// </summary>
        public FileHashesInfo HashesInfo { get; set; } = new();
        public bool AllFilesSuccess => FileResults.All(r => r.Success);
        public int SuccessCount => FileResults.Count(r => r.Success);
        public int FailedCount => FileResults.Count(r => !r.Success);
    }
}
