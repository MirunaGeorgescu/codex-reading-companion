using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Codex.Migrations
{
    /// <inheritdoc />
    public partial class AddBadgesToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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
        }
    }
}
