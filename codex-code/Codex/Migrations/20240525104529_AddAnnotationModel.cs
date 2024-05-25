using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Codex.Migrations
{
    /// <inheritdoc />
    public partial class AddAnnotationModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Annotations",
                columns: table => new
                {
                    AnnotationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    BuddyReadId = table.Column<int>(type: "int", nullable: true),
                    Comment = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Page = table.Column<int>(type: "int", nullable: false),
                    TimeStamp = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Annotations", x => x.AnnotationId);
                    table.ForeignKey(
                        name: "FK_Annotations_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Annotations_BuddyReads_BuddyReadId",
                        column: x => x.BuddyReadId,
                        principalTable: "BuddyReads",
                        principalColumn: "BuddyReadId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Annotations_BuddyReadId",
                table: "Annotations",
                column: "BuddyReadId");

            migrationBuilder.CreateIndex(
                name: "IX_Annotations_UserId",
                table: "Annotations",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Annotations");
        }
    }
}
