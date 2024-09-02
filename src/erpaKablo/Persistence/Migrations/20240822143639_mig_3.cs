using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class mig_3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CarouselId",
                table: "ImageFiles",
                type: "varchar(255)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Carousel",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(255)", nullable: false),
                    Name = table.Column<string>(type: "longtext", nullable: true),
                    Description = table.Column<string>(type: "longtext", nullable: true),
                    Order = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    DeletedDate = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Carousel", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_ImageFiles_CarouselId",
                table: "ImageFiles",
                column: "CarouselId");

            migrationBuilder.AddForeignKey(
                name: "FK_ImageFiles_Carousel_CarouselId",
                table: "ImageFiles",
                column: "CarouselId",
                principalTable: "Carousel",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ImageFiles_Carousel_CarouselId",
                table: "ImageFiles");

            migrationBuilder.DropTable(
                name: "Carousel");

            migrationBuilder.DropIndex(
                name: "IX_ImageFiles_CarouselId",
                table: "ImageFiles");

            migrationBuilder.DropColumn(
                name: "CarouselId",
                table: "ImageFiles");
        }
    }
}
