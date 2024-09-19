using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CodeShareBackend.Migrations
{
    /// <inheritdoc />
    public partial class @readonly : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "ExpiryDate",
                table: "CodeSnippets",
                type: "datetime2",
                nullable: true,
                defaultValue: new DateTime(2024, 9, 21, 13, 6, 59, 101, DateTimeKind.Local).AddTicks(6682),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true,
                oldDefaultValue: new DateTime(2024, 9, 20, 23, 42, 25, 266, DateTimeKind.Local).AddTicks(4949));

            migrationBuilder.AddColumn<bool>(
                name: "ReadOnly",
                table: "CodeSnippets",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReadOnly",
                table: "CodeSnippets");

            migrationBuilder.AlterColumn<DateTime>(
                name: "ExpiryDate",
                table: "CodeSnippets",
                type: "datetime2",
                nullable: true,
                defaultValue: new DateTime(2024, 9, 20, 23, 42, 25, 266, DateTimeKind.Local).AddTicks(4949),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true,
                oldDefaultValue: new DateTime(2024, 9, 21, 13, 6, 59, 101, DateTimeKind.Local).AddTicks(6682));
        }
    }
}
