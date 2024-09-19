namespace CodeShareBackend.Models {
    public class CodeSnippet {
        public int Id { get; set; }
        public string? Code { get; set; }
        public string UniqueId { get; set; } = string.Empty;
        public DateTime? ExpiryDate { get; set; }
        public string? UserId { get; set; }
        public int SelectedLangId { get; set; }
        public bool ReadOnly { get; set; }

        public UserCodeShare? User { get; set; }
        public ProgLanguage? SelectedLang { get; set; }
    }
}
