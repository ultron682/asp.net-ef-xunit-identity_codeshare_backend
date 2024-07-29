using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CodeShareBackend.Migrations
{
    /// <inheritdoc />
    public partial class addedExpiryDateDefaultvalur : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "ExpiryDate",
                table: "CodeSnippets",
                type: "datetime2",
                nullable: true,
                defaultValue: new DateTime(2024, 7, 29, 14, 18, 37, 312, DateTimeKind.Local).AddTicks(6399),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "ExpiryDate",
                table: "CodeSnippets",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true,
                oldDefaultValue: new DateTime(2024, 7, 29, 14, 18, 37, 312, DateTimeKind.Local).AddTicks(6399));
        }
    }
}
