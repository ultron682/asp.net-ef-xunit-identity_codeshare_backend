using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CodeShareBackend.Migrations
{
    /// <inheritdoc />
    public partial class auto_expiry_date : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "ExpiryDate",
                table: "CodeSnippets",
                type: "datetime2",
                nullable: true,
                defaultValueSql: "dateadd(day, 7, getdate())",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true,
                oldDefaultValue: new DateTime(2024, 9, 21, 13, 6, 59, 101, DateTimeKind.Local).AddTicks(6682));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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
                oldDefaultValueSql: "dateadd(day, 7, getdate())");
        }
    }
}
