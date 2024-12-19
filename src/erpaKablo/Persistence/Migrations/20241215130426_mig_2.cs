using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class mig_2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ProductImageFile_Alt",
                table: "ImageFiles",
                newName: "Title");

            migrationBuilder.RenameColumn(
                name: "BrandImageFile_Alt",
                table: "ImageFiles",
                newName: "License");

            migrationBuilder.AddColumn<string>(
                name: "Caption",
                table: "ImageFiles",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "ImageFiles",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "FileSize",
                table: "ImageFiles",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<string>(
                name: "Format",
                table: "ImageFiles",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "GeoLocation",
                table: "ImageFiles",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Height",
                table: "ImageFiles",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Width",
                table: "ImageFiles",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "ImageVersions",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    ImageFileId = table.Column<string>(type: "text", nullable: false),
                    Size = table.Column<string>(type: "text", nullable: false),
                    Width = table.Column<int>(type: "integer", nullable: false),
                    Height = table.Column<int>(type: "integer", nullable: false),
                    Format = table.Column<string>(type: "text", nullable: false),
                    Path = table.Column<string>(type: "text", nullable: false),
                    Storage = table.Column<string>(type: "text", nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    IsWebpVersion = table.Column<bool>(type: "boolean", nullable: false),
                    IsAvifVersion = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    DeletedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImageVersions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ImageVersions_ImageFiles_ImageFileId",
                        column: x => x.ImageFileId,
                        principalTable: "ImageFiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ImageVersions_ImageFileId",
                table: "ImageVersions",
                column: "ImageFileId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ImageVersions");

            migrationBuilder.DropColumn(
                name: "Caption",
                table: "ImageFiles");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "ImageFiles");

            migrationBuilder.DropColumn(
                name: "FileSize",
                table: "ImageFiles");

            migrationBuilder.DropColumn(
                name: "Format",
                table: "ImageFiles");

            migrationBuilder.DropColumn(
                name: "GeoLocation",
                table: "ImageFiles");

            migrationBuilder.DropColumn(
                name: "Height",
                table: "ImageFiles");

            migrationBuilder.DropColumn(
                name: "Width",
                table: "ImageFiles");

            migrationBuilder.RenameColumn(
                name: "Title",
                table: "ImageFiles",
                newName: "ProductImageFile_Alt");

            migrationBuilder.RenameColumn(
                name: "License",
                table: "ImageFiles",
                newName: "BrandImageFile_Alt");
        }
    }
}
