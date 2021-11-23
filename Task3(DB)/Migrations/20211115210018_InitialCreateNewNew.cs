using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Task3_DB_.Migrations
{
    public partial class InitialCreateNewNew : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Pictures_RectangleDB_rectangleId",
                table: "Pictures");

            migrationBuilder.DropTable(
                name: "RectangleDB");

            migrationBuilder.DropIndex(
                name: "IX_Pictures_rectangleId",
                table: "Pictures");

            migrationBuilder.DropColumn(
                name: "rectangleId",
                table: "Pictures");

            migrationBuilder.AddColumn<byte[]>(
                name: "rectangle",
                table: "Pictures",
                type: "BLOB",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "rectangle",
                table: "Pictures");

            migrationBuilder.AddColumn<int>(
                name: "rectangleId",
                table: "Pictures",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "RectangleDB",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    x1 = table.Column<float>(type: "REAL", nullable: false),
                    x2 = table.Column<float>(type: "REAL", nullable: false),
                    y1 = table.Column<float>(type: "REAL", nullable: false),
                    y2 = table.Column<float>(type: "REAL", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RectangleDB", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Pictures_rectangleId",
                table: "Pictures",
                column: "rectangleId");

            migrationBuilder.AddForeignKey(
                name: "FK_Pictures_RectangleDB_rectangleId",
                table: "Pictures",
                column: "rectangleId",
                principalTable: "RectangleDB",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
