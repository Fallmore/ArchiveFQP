using ArchiveFqp.Interfaces.ReferenceData;
using ArchiveFqp.Models.Settings.SettingsArchive;

namespace ArchiveFqp.Services.PdfRender
{
    public interface IPdfRenderService
    {
        Task<List<string>> RenderPdfToBase64Async(Stream pdfStream, CancellationToken cancellationToken, string? watermarkText = null, float? scale = 1.5f, IProgress<int>? progress = null);
        Task<Stream?> GetPdfStream(int idWork, IReferenceDataService refDataService, SettingsArchive settings);
    }
}
