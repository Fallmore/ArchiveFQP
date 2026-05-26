namespace ArchiveFqp.Models.FileUpload
{
    /// <summary>
    /// Динамический тип файла, который может быть настроен через Settings
    /// </summary>
    public class FileType
    {
        public string Key { get; set; } = string.Empty;  // Уникальный ключ типа файла (например, "ExplanatoryNoteWord")
        public string DisplayName { get; set; } = string.Empty; // Отображаемое имя (например, "Пояснительная записка (Word)")
        public string FilePrefix { get; set; } = string.Empty; // Префикс для имени файла
        public bool IsRequired { get; set; } = false; // Обязательный ли файл
        public string? Description { get; set; } // Описание для UI

        // Для совместимости со старым написанным кодом
        public static readonly FileType ExplanatoryNoteWord = new()
        {
            Key = "ExplanatoryNoteWord",
            DisplayName = "Пояснительная записка (Word)",
            FilePrefix = "explanation_word"
        };

        public static readonly FileType ExplanatoryNotePdf = new()
        {
            Key = "ExplanatoryNotePdf",
            DisplayName = "Пояснительная записка (Pdf)",
            FilePrefix = "explanation_pdf"
        };

        public static readonly FileType Presentation = new()
        {
            Key = "Presentation",
            DisplayName = "Презентация",
            FilePrefix = "presentation"
        };

        public static readonly FileType SourceCode = new()
        {
            Key = "SourceCode",
            DisplayName = "Исходный код",
            FilePrefix = "source",
        };

        public static readonly FileType DatabaseBackup = new()
        {
            Key = "DatabaseBackup",
            DisplayName = "База данных",
            FilePrefix = "database"
        };

        public static readonly FileType PasswordFile = new()
        {
            Key = "PasswordFile",
            DisplayName = "Пароли",
            FilePrefix = "passwords"
        };

        public static IEnumerable<FileType> GetDefaultFileTypes()
        {
            return
            [
                ExplanatoryNoteWord,
                ExplanatoryNotePdf,
                Presentation,
                SourceCode,
                DatabaseBackup,
                PasswordFile
            ];
        }

        public override bool Equals(object? obj)
        {
            return obj is FileType other && Key == other.Key;
        }

        public override int GetHashCode()
        {
            return Key.GetHashCode();
        }

        public override string ToString()
        {
            return DisplayName;
        }
    }
}
