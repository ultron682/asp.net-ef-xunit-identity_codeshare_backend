using Microsoft.EntityFrameworkCore;
using CodeShareBackend.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using System.Diagnostics;

namespace CodeShareBackend.Data
{
    public class ApplicationDbContext : IdentityDbContext<User>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            //base.OnConfiguring(optionsBuilder);
            optionsBuilder.LogTo(message => Debug.WriteLine(message));
        }
        public DbSet<CodeSnippet> CodeSnippets { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>()
                .HasMany(u => u.CodeSnippets)
                .WithOne(c => c.User)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            //modelBuilder.Entity<CodeSnippet>(builder =>
            //{
            //    builder.HasOne<ProgLanguage>()
            //    .WithOne()
            //    .HasForeignKey(c => c.SelectedLang)
            //    .OnDelete(DeleteBehavior.Cascade);
            //});

            modelBuilder.Entity<ProgLanguage>(builder =>
            {
                builder
                .HasOne<CodeSnippet>()
                .WithOne();

                builder.HasData(
                    new ProgLanguage() { Id = 0, Name = "javascript" },
                    new ProgLanguage() { Id = 0, Name = "xml" },
                    new ProgLanguage() { Id = 0, Name = "css" },
                    new ProgLanguage() { Id = 0, Name = "go" },
                    new ProgLanguage() { Id = 0, Name = "php" },
                    new ProgLanguage() { Id = 0, Name = "python" },
                    new ProgLanguage() { Id = 0, Name = "sql" },
                    new ProgLanguage() { Id = 0, Name = "swift" });
            });
        }
    }
}
