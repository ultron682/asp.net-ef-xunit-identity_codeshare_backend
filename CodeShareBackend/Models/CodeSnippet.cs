namespace CodeShareBackend.Models
{
    public class CodeSnippet
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string UniqueId { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public User? Owner { get; set; }
        public string? OwnerId { get; set; }
    }
}
