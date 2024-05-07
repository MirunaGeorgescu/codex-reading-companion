using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Codex.Migrations
{
    /// <inheritdoc />
    public partial class UpdateReadingBadgeModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "BadgeName",
                table: "ReadingBadges",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "BadgeImage",
                table: "ReadingBadges",
                newName: "Image");

            migrationBuilder.RenameColumn(
                name: "BadgeDescription",
                table: "ReadingBadges",
                newName: "Description");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Name",
                table: "ReadingBadges",
                newName: "BadgeName");

            migrationBuilder.RenameColumn(
                name: "Image",
                table: "ReadingBadges",
                newName: "BadgeImage");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "ReadingBadges",
                newName: "BadgeDescription");
        }
    }
}
