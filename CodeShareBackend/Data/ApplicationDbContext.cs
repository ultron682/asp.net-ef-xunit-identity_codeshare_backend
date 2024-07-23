﻿using Microsoft.EntityFrameworkCore;
using CodeShareBackend.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace CodeShareBackend.Data
{
    public class ApplicationDbContext : IdentityDbContext<User>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }
        public DbSet<CodeSnippet> CodeSnippets { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<CodeSnippet>(eb =>
            {
                eb.HasOne<User>()
                    .WithMany()
                    .HasForeignKey(c => c.OwnerId);
            });
        }
    }
}