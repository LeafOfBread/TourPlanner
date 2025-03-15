using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TourPlannerClasses.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedTourLogs1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Content",
                table: "TourLogs",
                newName: "Comment");

            migrationBuilder.AddColumn<DateTime>(
                name: "Date",
                table: "TourLogs",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "Difficulty",
                table: "TourLogs",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Date",
                table: "TourLogs");

            migrationBuilder.DropColumn(
                name: "Difficulty",
                table: "TourLogs");

            migrationBuilder.RenameColumn(
                name: "Comment",
                table: "TourLogs",
                newName: "Content");
        }
    }
}
