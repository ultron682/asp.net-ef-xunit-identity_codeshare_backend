using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CodeShareBackend.Migrations
{
    /// <inheritdoc />
    public partial class expirydate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ExpiryDate",
                table: "CodeSnippets",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExpiryDate",
                table: "CodeSnippets");
        }
    }
}
