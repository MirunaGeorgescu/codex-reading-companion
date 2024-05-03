using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Codex.Migrations
{
    /// <inheritdoc />
    public partial class AddReadingbadgeModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ReadingBadges",
                columns: table => new
                {
                    BadgeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BadgeName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BadgeDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BadgeImage = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReadingBadges", x => x.BadgeId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReadingBadges");
        }
    }
}
