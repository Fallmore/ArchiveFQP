using ArchiveFqp.Interfaces.PdfRender;
using ArchiveFqp.Interfaces.ReferenceData;
using ArchiveFqp.Models.Database;
using ArchiveFqp.Models.Settings.SettingsArchive;
using SkiaSharp;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Rendering.Skia;

namespace ArchiveFqp.Services.PdfRender
{
    public class PdfSciaRenderService : IPdfRenderService
    {
        public PdfSciaRenderService(IWebHostEnvironment environment, SettingsArchive settings)
        {
            if (string.IsNullOrEmpty(settings.FilesRootPath))
            {
                settings.FilesRootPath = environment.ContentRootPath;
            }
        }
        
        public Task<List<string>> RenderPdfToBase64Async(Stream pdfStream, CancellationToken cancellationToken,
            string? watermarkText = "КОНФИДЕНЦИАЛЬНО", float? scale = 1.5f,
            IProgress<int>? progress = null)
        {
            List<string> renderedPages = [];

            // Критически важно: использовать SkiaRenderingParsingOptions для рендеринга
            using PdfDocument document = PdfDocument.Open(pdfStream, SkiaRenderingParsingOptions.Instance);
            // Инициализация фабрики страниц для рендеринга
            document.AddSkiaPageFactory();
            progress?.Report(0);
            try
            {
                for (int pageNumber = 1; pageNumber <= document.NumberOfPages && !cancellationToken.IsCancellationRequested; pageNumber++)
                {
                    // 1. Рендерим страницу в SKBitmap с белым фоном
                    using SKBitmap pageBitmap = document.GetPageAsSKBitmap(pageNumber, scale!.Value, SKColors.White);

                    // 2. Накладываем водяной знак на изображение
                    ApplyWatermark(pageBitmap, watermarkText!);

                    // 3. Кодируем измененное изображение в PNG и конвертируем в base64
                    using SKImage image = SKImage.FromBitmap(pageBitmap);
                    using SKData data = image.Encode(SKEncodedImageFormat.Png, 90);

                    renderedPages.Add(Convert.ToBase64String(data.ToArray()));
                    var percentComplete = (int)((double)pageNumber / document.NumberOfPages * 100);
                    progress?.Report(percentComplete);
                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                // корректный выход при остановке
            }

            return Task.FromResult(renderedPages);
        }

        /// <summary>
        /// Рисует диагональный полупрозрачный водяной знак по всему изображению.
        /// </summary>
        private static void ApplyWatermark(SKBitmap bitmap, string watermarkText)
        {
            using SKCanvas canvas = new(bitmap);
            using SKPaint paint = new();
            using SKFont font = new();

            // Настройка шрифта
            SKTypeface typeface = SKTypeface.FromFamilyName("Arial", SKFontStyleWeight.Bold, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright);
            font.Typeface = typeface;
            font.Size = Math.Max(bitmap.Width, bitmap.Height) / 50; // Адаптивный размер
            font.Subpixel = true;
            font.Hinting = SKFontHinting.Full;

            // Настройка кисти
            paint.Color = new SKColor(255, 0, 0, 60); // Красный, полупрозрачный
            paint.IsAntialias = true;

            // --- Поворот наискосок и заполнение ---
            canvas.Save();
            // Центрируем систему координат для поворота
            canvas.Translate(bitmap.Width / 2f, bitmap.Height / 2f);
            canvas.RotateDegrees(-35); // Угол поворота

            // Измеряем текст, чтобы рассчитать шаг сетки
            float textWidth = font.MeasureText(watermarkText);
            float stepX = textWidth + (textWidth * 0.1f);
            float stepY = font.Size * 10;

            // Рисуем повторяющийся текст, покрывая все изображение (и даже немного за его пределами)
            float diagonal = (float)Math.Sqrt(bitmap.Width * bitmap.Width + bitmap.Height * bitmap.Height);
            float start = -diagonal;
            float end = diagonal;

            for (float y = start; y < end; y += stepY)
            {
                for (float x = start; x < end; x += stepX)
                {
                    canvas.DrawText(watermarkText, x, y, SKTextAlign.Center, font, paint);
                }
            }

            canvas.Restore();
        }

        public async Task<Stream?> GetPdfStream(int idWork, IReferenceDataService refDataService, SettingsArchive settings)
        {
            Работа? work = (await refDataService.GetAsync<Работа>()).FirstOrDefault(x => x.IdРаботы == idWork);
            if (work == null || work.Местоположение == null) return null;

            string path = Path.Combine(settings.FilesRootPath, settings.FolderDataName, settings.FolderWorksName, work.Местоположение, $"{settings.AllowedFiles.Keys.FirstOrDefault(k => k == "Пояснительная записка (Pdf)")}.pdf");
            return File.OpenRead(path);
        }
    }
}
