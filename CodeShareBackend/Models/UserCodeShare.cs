using Microsoft.AspNetCore.Identity;

namespace CodeShareBackend.Models
{
    public class UserCodeShare: IdentityUser
    {
        public List<CodeSnippet> CodeSnippets { get; set; } = new List<CodeSnippet>();
    }
}
