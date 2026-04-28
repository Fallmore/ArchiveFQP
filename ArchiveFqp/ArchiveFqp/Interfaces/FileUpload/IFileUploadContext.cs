using Microsoft.AspNetCore.Components.Forms;

namespace ArchiveFqp.Interfaces.FileUpload
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
}
