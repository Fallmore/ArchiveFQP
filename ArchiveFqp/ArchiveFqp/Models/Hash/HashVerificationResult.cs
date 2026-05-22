namespace ArchiveFqp.Models.Hash
{
    /// <summary>
    /// Результат проверки хеша
    /// </summary>
    public class HashVerificationResult
    {
        public bool IsValid { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
