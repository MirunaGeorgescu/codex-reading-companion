using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Codex.Migrations
{
    /// <inheritdoc />
    public partial class UdateReadingChallange2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Books_readingChallenges_ReadingChallengeId",
                table: "Books");

            migrationBuilder.DropForeignKey(
                name: "FK_readingChallenges_AspNetUsers_UserId",
                table: "readingChallenges");

            migrationBuilder.DropPrimaryKey(
                name: "PK_readingChallenges",
                table: "readingChallenges");

            migrationBuilder.RenameTable(
                name: "readingChallenges",
                newName: "ReadingChallenges");

            migrationBuilder.RenameIndex(
                name: "IX_readingChallenges_UserId",
                table: "ReadingChallenges",
                newName: "IX_ReadingChallenges_UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ReadingChallenges",
                table: "ReadingChallenges",
                column: "ReadingChallengeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Books_ReadingChallenges_ReadingChallengeId",
                table: "Books",
                column: "ReadingChallengeId",
                principalTable: "ReadingChallenges",
                principalColumn: "ReadingChallengeId");

            migrationBuilder.AddForeignKey(
                name: "FK_ReadingChallenges_AspNetUsers_UserId",
                table: "ReadingChallenges",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Books_ReadingChallenges_ReadingChallengeId",
                table: "Books");

            migrationBuilder.DropForeignKey(
                name: "FK_ReadingChallenges_AspNetUsers_UserId",
                table: "ReadingChallenges");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ReadingChallenges",
                table: "ReadingChallenges");

            migrationBuilder.RenameTable(
                name: "ReadingChallenges",
                newName: "readingChallenges");

            migrationBuilder.RenameIndex(
                name: "IX_ReadingChallenges_UserId",
                table: "readingChallenges",
                newName: "IX_readingChallenges_UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_readingChallenges",
                table: "readingChallenges",
                column: "ReadingChallengeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Books_readingChallenges_ReadingChallengeId",
                table: "Books",
                column: "ReadingChallengeId",
                principalTable: "readingChallenges",
                principalColumn: "ReadingChallengeId");

            migrationBuilder.AddForeignKey(
                name: "FK_readingChallenges_AspNetUsers_UserId",
                table: "readingChallenges",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
