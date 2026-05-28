using ArchiveFqp.Interfaces.FileUpload;
using ArchiveFqp.Models.DTO.Structure;
using Microsoft.AspNetCore.Components.Forms;

namespace ArchiveFqp.Models.FileUpload
{
    /// <summary>
    /// Контекст загрузки файла
    /// </summary>
    public class WorkUploadContext : IFileUploadContext
    {
        public StructureDto Structure { get; set; } = new();
        public string WorkType { get; set; } = string.Empty;
        public int Year { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string WorkTitle { get; set; } = string.Empty;
        public bool IsTemp { get; set; } = false;
        public Dictionary<string, List<IBrowserFile>> Files { get; set; } = new();


        // Для обратной совместимости со старым кодом
        public IBrowserFile? ExplanatoryNoteWord
        {
            get => Files.TryGetValue(FileType.ExplanatoryNoteWord.Key, out var list) && list.Any() ? list.First() : null;
            set => SetFile(FileType.ExplanatoryNoteWord.Key, value);
        }

        public IBrowserFile? ExplanatoryNotePdf
        {
            get => Files.TryGetValue(FileType.ExplanatoryNotePdf.Key, out var list) && list.Any() ? list.First() : null;
            set => SetFile(FileType.ExplanatoryNotePdf.Key, value);
        }

        public IBrowserFile? Presentation
        {
            get => Files.TryGetValue(FileType.Presentation.Key, out var list) && list.Any() ? list.First() : null;
            set => SetFile(FileType.Presentation.Key, value);
        }

        public List<IBrowserFile> SourceCodeFiles
        {
            get => Files.TryGetValue(FileType.SourceCode.Key, out var list) ? list : new();
            set => SetFiles(FileType.SourceCode.Key, value);
        }

        public IBrowserFile? DatabaseBackup
        {
            get => Files.TryGetValue(FileType.DatabaseBackup.Key, out var list) && list.Any() ? list.First() : null;
            set => SetFile(FileType.DatabaseBackup.Key, value);
        }

        public IBrowserFile? PasswordFile
        {
            get => Files.TryGetValue(FileType.PasswordFile.Key, out var list) && list.Any() ? list.First() : null;
            set => SetFile(FileType.PasswordFile.Key, value);
        }

        private void SetFile(string key, IBrowserFile? file)
        {
            if (file == null)
            {
                Files.Remove(key);
            }
            else
            {
                Files[key] = new List<IBrowserFile> { file };
            }
        }

        private void SetFiles(string key, List<IBrowserFile> files)
        {
            if (files == null || !files.Any())
            {
                Files.Remove(key);
            }
            else
            {
                Files[key] = files;
            }
        }

        /// <summary>
        /// Добавить файл определенного типа
        /// </summary>
        public void AddFile(string fileTypeKey, IBrowserFile file)
        {
            if (!Files.TryGetValue(fileTypeKey, out List<IBrowserFile>? value))
            {
                value = new List<IBrowserFile>();
                Files[fileTypeKey] = value;
            }

            value.Add(file);
        }

        /// <summary>
        /// Получить файлы определенного типа
        /// </summary>
        public List<IBrowserFile> GetFiles(string fileTypeKey)
        {
            return Files.TryGetValue(fileTypeKey, out var files) ? files : new();
        }

        /// <summary>
        /// Удалить файл определенного типа
        /// </summary>
        public bool RemoveFile(string fileTypeKey, int index = 0)
        {
            if (Files.TryGetValue(fileTypeKey, out var files) && index < files.Count)
            {
                files.RemoveAt(index);
                if (files.Count == 0)
                {
                    Files.Remove(fileTypeKey);
                }
                return true;
            }
            return false;
        }
    }
}
