using ArchiveFqp.Interfaces.ReferenceData;
using ArchiveFqp.Models.Settings.SettingsArchive;

namespace ArchiveFqp.Interfaces.PdfRender
{
    public interface IPdfRenderService
    {
        /// <summary>
        /// Возвращает список страниц PDF в виде base64 PNG с наложенным водяным знаком.
        /// </summary>
        /// <param name="pdfStream">Поток с исходным PDF-файлом.</param>
        /// <param name="watermarkText">Текст водяного знака.</param>
        /// <param name="scale">Масштаб рендеринга (1.0f = 72 DPI, 2.0f = 144 DPI).</param>
        /// <returns>Список строк, где каждая строка — base64-изображение страницы.</returns>
        Task<List<string>> RenderPdfToBase64Async(Stream pdfStream, CancellationToken cancellationToken, string? watermarkText = null, float? scale = 1.5f, IProgress<int>? progress = null);

        Task<Stream?> GetPdfStream(int idWork, IReferenceDataService refDataService, SettingsArchive settings);
    }
}
