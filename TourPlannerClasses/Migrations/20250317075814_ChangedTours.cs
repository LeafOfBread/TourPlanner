using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TourPlannerClasses.Migrations
{
    /// <inheritdoc />
    public partial class ChangedTours : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "imagePath",
                table: "Tours");

            migrationBuilder.RenameColumn(
                name: "to",
                table: "Tours",
                newName: "To");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "Tours",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "from",
                table: "Tours",
                newName: "From");

            migrationBuilder.RenameColumn(
                name: "duration",
                table: "Tours",
                newName: "Duration");

            migrationBuilder.RenameColumn(
                name: "distance",
                table: "Tours",
                newName: "Distance");

            migrationBuilder.RenameColumn(
                name: "description",
                table: "Tours",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "Tours",
                newName: "Id");

            migrationBuilder.AddColumn<int>(
                name: "Transport",
                table: "Tours",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Transport",
                table: "Tours");

            migrationBuilder.RenameColumn(
                name: "To",
                table: "Tours",
                newName: "to");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Tours",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "From",
                table: "Tours",
                newName: "from");

            migrationBuilder.RenameColumn(
                name: "Duration",
                table: "Tours",
                newName: "duration");

            migrationBuilder.RenameColumn(
                name: "Distance",
                table: "Tours",
                newName: "distance");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "Tours",
                newName: "description");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Tours",
                newName: "id");

            migrationBuilder.AddColumn<string>(
                name: "imagePath",
                table: "Tours",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
