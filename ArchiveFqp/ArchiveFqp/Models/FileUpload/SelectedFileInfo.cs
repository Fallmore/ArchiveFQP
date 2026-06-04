using Microsoft.AspNetCore.Components.Forms;

namespace ArchiveFqp.Models.FileUpload
{
    public class SelectedFileInfo
    {
        public string Name { get; set; } = "";
        public long Size { get; set; }
        public string Extension { get; set; } = "";
        public string RelativePath { get; set; } = "";
        public string? DetectedTypeKey { get; set; }
        public string? SelectedTypeKey { get; set; }
        public IBrowserFile? BrowserFile { get; set; }

        public bool IsTempFile { get; set; } = false;
        public TempFileInfo? TempFileInfo { get; set; }

        public string DisplayDetectedType => DetectedTypeKey ?? "Не определен";
        public string DisplaySelectedType => SelectedTypeKey ?? "Не выбран";
    }
}
