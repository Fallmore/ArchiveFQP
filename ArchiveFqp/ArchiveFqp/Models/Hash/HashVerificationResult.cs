namespace ArchiveFqp.Models.Hash
{
    /// <summary>
    /// Результат проверки хэша
    /// </summary>
    public class HashVerificationResult
    {
        public bool IsValid { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
