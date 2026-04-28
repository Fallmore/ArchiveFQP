namespace ArchiveFqp.Models.Hash
{
    /// <summary>
    /// Информация о хэшах файлов
    /// </summary>
    public class FileHashesInfo
    {
        /// <summary>
        /// Словарь хэшей файлов (имя файла -> хэш)
        /// </summary>
        public Dictionary<string, string> FileHashes { get; set; } = new();

        /// <summary>
        /// Составной хэш (ЭЦП) всех файлов
        /// </summary>
        public string CompositeHash { get; set; } = string.Empty;

        /// <summary>
        /// Время вычисления хэшей
        /// </summary>
        public DateTime CalculatedAt { get; set; }

        /// <summary>
        /// Количество файлов
        /// </summary>
        public int FileCount => FileHashes.Count;

        /// <summary>
        /// Общий размер файлов в байтах
        /// </summary>
        public long TotalSize { get; set; }
    }
}
