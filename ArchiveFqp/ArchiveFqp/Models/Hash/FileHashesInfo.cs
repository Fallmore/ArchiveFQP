namespace ArchiveFqp.Models.Hash
{
    /// <summary>
    /// Информация о хешах файлов
    /// </summary>
    public class FileHashesInfo
    {
        /// <summary>
        /// Словарь хешей файлов (имя файла -> хеш)
        /// </summary>
        public Dictionary<string, string> FileHashes { get; set; } = new();

        /// <summary>
        /// Составной хеш (ЭЦП) всех файлов
        /// </summary>
        public string CompositeHash { get; set; } = string.Empty;

        /// <summary>
        /// Время вычисления хешей
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
