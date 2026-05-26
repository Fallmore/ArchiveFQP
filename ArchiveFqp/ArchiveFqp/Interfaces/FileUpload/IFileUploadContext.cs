using ArchiveFqp.Models.DTO.Structure;
using Microsoft.AspNetCore.Components.Forms;

namespace ArchiveFqp.Interfaces.FileUpload
{
    /// <summary>
    /// Интерфейс контекста загрузки файла
    /// </summary>
    public interface IFileUploadContext
    {
        StructureDto Structure { get; }
        string WorkType { get; }
        int Year { get; }
        string StudentName { get; }
        string WorkTitle { get; }
        Dictionary<string, List<IBrowserFile>> Files { get; }
    }
}
