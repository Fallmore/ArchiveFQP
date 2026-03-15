using Microsoft.AspNetCore.Components.Forms;
using System.ComponentModel.DataAnnotations;

namespace ArchiveFqp.Services.FileUpload
{
    /// <summary>
    /// Интерфейс контекста загрузки файла
    /// </summary>
    public interface IFileUploadContext
    {
        string Institute { get; }
        string Department { get; }
        string UgsnStandart { get; }
        string Direction { get; }
        string Profile { get; }
        string WorkType { get; }
        int Year { get; }
        string StudentName { get; }
        string WorkTitle { get; }
        IEnumerable<IBrowserFile> Files { get; }
    }

    /// <summary>
    /// Контекст загрузки файла
    /// </summary>
    public class WorkUploadContext : IFileUploadContext
    {
        public string Institute { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string UgsnStandart { get; set; } = string.Empty;
        public string Direction { get; set; } = string.Empty;
        public string Profile { get; set; } = string.Empty;
        public string WorkType { get; set; } = string.Empty;
        public int Year { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string WorkTitle { get; set; } = string.Empty;
        public IEnumerable<IBrowserFile> Files { get; set; } = [];

        // Специфические для работы файлы
        public IBrowserFile? ExplanatoryNoteWord { get; set; }
        public IBrowserFile? ExplanatoryNotePdf { get; set; }
        public IBrowserFile? Presentation { get; set; }
        public IEnumerable<IBrowserFile> SourceCodeFiles { get; set; } = [];
        public IBrowserFile? DatabaseBackup { get; set; }
        public IBrowserFile? PasswordFile { get; set; }
    }
}
