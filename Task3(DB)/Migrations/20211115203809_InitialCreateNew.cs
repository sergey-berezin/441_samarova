using Microsoft.EntityFrameworkCore.Migrations;

namespace Task3_DB_.Migrations
{
    public partial class InitialCreateNew : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "coordinate",
                table: "RectangleDB",
                newName: "y2");

            migrationBuilder.AddColumn<float>(
                name: "x1",
                table: "RectangleDB",
                type: "REAL",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<float>(
                name: "x2",
                table: "RectangleDB",
                type: "REAL",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<float>(
                name: "y1",
                table: "RectangleDB",
                type: "REAL",
                nullable: false,
                defaultValue: 0f);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "x1",
                table: "RectangleDB");

            migrationBuilder.DropColumn(
                name: "x2",
                table: "RectangleDB");

            migrationBuilder.DropColumn(
                name: "y1",
                table: "RectangleDB");

            migrationBuilder.RenameColumn(
                name: "y2",
                table: "RectangleDB",
                newName: "coordinate");
        }
    }
}
