using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TourPlannerClasses.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedTourLogsFixed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Rating",
                table: "TourLogs",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "TotalDistance",
                table: "TourLogs",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "TotalTime",
                table: "TourLogs",
                type: "interval",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Rating",
                table: "TourLogs");

            migrationBuilder.DropColumn(
                name: "TotalDistance",
                table: "TourLogs");

            migrationBuilder.DropColumn(
                name: "TotalTime",
                table: "TourLogs");
        }
    }
}
