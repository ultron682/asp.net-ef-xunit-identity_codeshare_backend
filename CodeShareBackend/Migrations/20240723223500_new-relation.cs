using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CodeShareBackend.Migrations
{
    /// <inheritdoc />
    public partial class newrelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CodeSnippets_AspNetUsers_OwnerId1",
                table: "CodeSnippets");

            migrationBuilder.DropIndex(
                name: "IX_CodeSnippets_OwnerId1",
                table: "CodeSnippets");

            migrationBuilder.DropColumn(
                name: "OwnerId1",
                table: "CodeSnippets");

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "CodeSnippets",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "CodeSnippets",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OwnerId1",
                table: "CodeSnippets",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CodeSnippets_OwnerId1",
                table: "CodeSnippets",
                column: "OwnerId1");

            migrationBuilder.AddForeignKey(
                name: "FK_CodeSnippets_AspNetUsers_OwnerId1",
                table: "CodeSnippets",
                column: "OwnerId1",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
