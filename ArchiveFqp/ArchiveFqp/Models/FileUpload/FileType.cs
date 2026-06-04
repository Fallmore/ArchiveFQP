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
            Key = "Пояснительная записка (Word)",
            DisplayName = "Пояснительная записка (Word)",
            FilePrefix = "Пояснительная записка (Word)"
        };

        public static readonly FileType ExplanatoryNotePdf = new()
        {
            Key = "Пояснительная записка (Pdf)",
            DisplayName = "Пояснительная записка (Pdf)",
            FilePrefix = "Пояснительная записка (Pdf)"
        };

        public static readonly FileType Presentation = new()
        {
            Key = "Презентация",
            DisplayName = "Презентация",
            FilePrefix = "Презентация"
        };

        public static readonly FileType SourceCode = new()
        {
            Key = "Исходный код",
            DisplayName = "Исходный код",
            FilePrefix = "Исходный код",
        };

        public static readonly FileType DatabaseBackup = new()
        {
            Key = "База данных",
            DisplayName = "База данных",
            FilePrefix = "База данных"
        };

        public static readonly FileType PasswordFile = new()
        {
            Key = "Пароли",
            DisplayName = "Пароли",
            FilePrefix = "Пароли"
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
