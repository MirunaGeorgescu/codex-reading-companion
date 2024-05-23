using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Codex.Migrations
{
    /// <inheritdoc />
    public partial class AddMoreStreakInfo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastReadingDate",
                table: "AspNetUsers");

            migrationBuilder.AddColumn<DateOnly>(
                name: "LastStreakDay",
                table: "AspNetUsers",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.AddColumn<DateOnly>(
                name: "LastUpdate",
                table: "AspNetUsers",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.AddColumn<int>(
                name: "PagesReadToday",
                table: "AspNetUsers",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastStreakDay",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "LastUpdate",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "PagesReadToday",
                table: "AspNetUsers");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastReadingDate",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true);
        }
    }
}
