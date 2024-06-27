using Microsoft.EntityFrameworkCore;
using CodeShareBackend.Models;

namespace CodeShareBackend.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<CodeSnippet> CodeSnippets { get; set; }
    }
}
