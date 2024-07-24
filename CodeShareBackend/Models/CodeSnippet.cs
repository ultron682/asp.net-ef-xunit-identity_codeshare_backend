namespace CodeShareBackend.Models
{
    public class CodeSnippet
    {
        public int Id { get; set; }
        public string? Code { get; set; }
        public string UniqueId { get; set; } = string.Empty;
        public DateTime? ExpiryDate { get; set; }
        public string? UserId { get; set; }
        public User? User { get; set; }

    }
}
