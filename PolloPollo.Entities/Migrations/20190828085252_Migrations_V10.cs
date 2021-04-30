using Microsoft.EntityFrameworkCore.Migrations;

namespace PolloPollo.Entities.Migrations
{
    public partial class Migrations_V10 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "City",
                table: "Users");

            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "Producers",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Street",
                table: "Producers",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "StreetNumber",
                table: "Producers",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Zipcode",
                table: "Producers",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DonationDate",
                table: "Applications",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "City",
                table: "Producers");

            migrationBuilder.DropColumn(
                name: "Street",
                table: "Producers");

            migrationBuilder.DropColumn(
                name: "StreetNumber",
                table: "Producers");

            migrationBuilder.DropColumn(
                name: "Zipcode",
                table: "Producers");

            migrationBuilder.DropColumn(
                name: "DonationDate",
                table: "Applications");

            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "Users",
                nullable: true);
        }
    }
}
