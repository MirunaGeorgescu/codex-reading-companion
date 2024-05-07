using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Codex.Migrations
{
    /// <inheritdoc />
    public partial class UpdateBadgesToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BadgesEarned_AspNetUsers_ApplicationUserId",
                table: "BadgesEarned");

            migrationBuilder.DropIndex(
                name: "IX_BadgesEarned_ApplicationUserId",
                table: "BadgesEarned");

            migrationBuilder.DropColumn(
                name: "ApplicationUserId",
                table: "BadgesEarned");

            migrationBuilder.AddColumn<string>(
                name: "ApplicationUserId",
                table: "ReadingBadges",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReadingBadges_ApplicationUserId",
                table: "ReadingBadges",
                column: "ApplicationUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_ReadingBadges_AspNetUsers_ApplicationUserId",
                table: "ReadingBadges",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ReadingBadges_AspNetUsers_ApplicationUserId",
                table: "ReadingBadges");

            migrationBuilder.DropIndex(
                name: "IX_ReadingBadges_ApplicationUserId",
                table: "ReadingBadges");

            migrationBuilder.DropColumn(
                name: "ApplicationUserId",
                table: "ReadingBadges");

            migrationBuilder.AddColumn<string>(
                name: "ApplicationUserId",
                table: "BadgesEarned",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_BadgesEarned_ApplicationUserId",
                table: "BadgesEarned",
                column: "ApplicationUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_BadgesEarned_AspNetUsers_ApplicationUserId",
                table: "BadgesEarned",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
