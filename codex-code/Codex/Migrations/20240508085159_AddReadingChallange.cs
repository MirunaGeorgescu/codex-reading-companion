using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Codex.Migrations
{
    /// <inheritdoc />
    public partial class AddReadingChallange : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ReadingChallangeId",
                table: "Books",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "readingChallanges",
                columns: table => new
                {
                    ReadingChallangeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: false),
                    TargetNumber = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_readingChallanges", x => x.ReadingChallangeId);
                    table.ForeignKey(
                        name: "FK_readingChallanges_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Books_ReadingChallangeId",
                table: "Books",
                column: "ReadingChallangeId");

            migrationBuilder.CreateIndex(
                name: "IX_readingChallanges_UserId",
                table: "readingChallanges",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Books_readingChallanges_ReadingChallangeId",
                table: "Books",
                column: "ReadingChallangeId",
                principalTable: "readingChallanges",
                principalColumn: "ReadingChallangeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Books_readingChallanges_ReadingChallangeId",
                table: "Books");

            migrationBuilder.DropTable(
                name: "readingChallanges");

            migrationBuilder.DropIndex(
                name: "IX_Books_ReadingChallangeId",
                table: "Books");

            migrationBuilder.DropColumn(
                name: "ReadingChallangeId",
                table: "Books");
        }
    }
}
