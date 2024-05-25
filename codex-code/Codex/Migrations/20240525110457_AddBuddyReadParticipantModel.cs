using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Codex.Migrations
{
    /// <inheritdoc />
    public partial class AddBuddyReadParticipantModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BuddyReadsParticipants",
                columns: table => new
                {
                    BuddyReadId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BuddyReadsParticipants", x => new { x.BuddyReadId, x.UserId });
                    table.ForeignKey(
                        name: "FK_BuddyReadsParticipants_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BuddyReadsParticipants_BuddyReads_BuddyReadId",
                        column: x => x.BuddyReadId,
                        principalTable: "BuddyReads",
                        principalColumn: "BuddyReadId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BuddyReadsParticipants_UserId",
                table: "BuddyReadsParticipants",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BuddyReadsParticipants");
        }
    }
}
