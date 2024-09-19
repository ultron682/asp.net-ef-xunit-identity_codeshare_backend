using Microsoft.EntityFrameworkCore;
using CodeShareBackend.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using System.Diagnostics;

namespace CodeShareBackend.Data {
    public class ApplicationDbContext : IdentityDbContext<UserCodeShare> {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) {

        }

        public DbSet<CodeSnippet> CodeSnippets { get; set; }
        public DbSet<ProgLanguage> ProgLanguages { get; set; }

        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //{
        //    //base.OnConfiguring(optionsBuilder);
        //    optionsBuilder.LogTo(message => Debug.WriteLine(message));
        //}

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<UserCodeShare>()
                .HasMany(u => u.CodeSnippets)
                .WithOne(c => c.User)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CodeSnippet>(builder => {
                builder
                .HasKey(p => p.Id);

                builder
                .HasOne(p => p.SelectedLang)
                .WithMany()
                .HasForeignKey(c => c.SelectedLangId)
                .OnDelete(DeleteBehavior.Restrict);

                builder
                .Property(c => c.SelectedLangId)
                .HasDefaultValue(1);

                //current date plus 7 days
                builder.Property(p => p.ExpiryDate).HasDefaultValueSql("dateadd(day, 7, getdate())");
            });

            modelBuilder.Entity<ProgLanguage>(builder => {
                builder
                .HasKey(p => p.Id);

                builder.Property(p => p.Name)
                .IsRequired();

                builder.HasData(
                    new ProgLanguage() { Id = 1, Name = "javascript" },
                    new ProgLanguage() { Id = 2, Name = "xml" },
                    new ProgLanguage() { Id = 3, Name = "css" },
                    new ProgLanguage() { Id = 4, Name = "go" },
                    new ProgLanguage() { Id = 5, Name = "php" },
                    new ProgLanguage() { Id = 6, Name = "python" },
                    new ProgLanguage() { Id = 7, Name = "sql" },
                    new ProgLanguage() { Id = 8, Name = "swift" });
            });
        }
    }
}
