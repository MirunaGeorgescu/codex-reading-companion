using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Codex.Migrations
{
    /// <inheritdoc />
    public partial class AddCriteriaForBadges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CriteriaType",
                table: "ReadingBadges",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CriteriaValue",
                table: "ReadingBadges",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "TargetName",
                table: "ReadingBadges",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CriteriaType",
                table: "ReadingBadges");

            migrationBuilder.DropColumn(
                name: "CriteriaValue",
                table: "ReadingBadges");

            migrationBuilder.DropColumn(
                name: "TargetName",
                table: "ReadingBadges");
        }
    }
}
