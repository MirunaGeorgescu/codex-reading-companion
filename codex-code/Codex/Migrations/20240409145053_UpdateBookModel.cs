using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Codex.Migrations
{
    /// <inheritdoc />
    public partial class UpdateBookModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Synopsys",
                table: "Books",
                newName: "Synopsis");

            migrationBuilder.RenameColumn(
                name: "Genere",
                table: "Books",
                newName: "Genre");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Synopsis",
                table: "Books",
                newName: "Synopsys");

            migrationBuilder.RenameColumn(
                name: "Genre",
                table: "Books",
                newName: "Genere");
        }
    }
}
