using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class mig_5 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PhoneNumberId",
                table: "Orders",
                type: "varchar(255)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Orders_PhoneNumberId",
                table: "Orders",
                column: "PhoneNumberId");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_PhoneNumbers_PhoneNumberId",
                table: "Orders",
                column: "PhoneNumberId",
                principalTable: "PhoneNumbers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_PhoneNumbers_PhoneNumberId",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_PhoneNumberId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "PhoneNumberId",
                table: "Orders");
        }
    }
}
