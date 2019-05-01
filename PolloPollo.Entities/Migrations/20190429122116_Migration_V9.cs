using Microsoft.EntityFrameworkCore.Migrations;

namespace PolloPollo.Entities.Migrations
{
    public partial class Migration_V9 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Wallet",
                table: "Producers");

            migrationBuilder.AddColumn<string>(
                name: "DeviceAddress",
                table: "Producers",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PairingSecret",
                table: "Producers",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "WalletAddress",
                table: "Producers",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeviceAddress",
                table: "Producers");

            migrationBuilder.DropColumn(
                name: "PairingSecret",
                table: "Producers");

            migrationBuilder.DropColumn(
                name: "WalletAddress",
                table: "Producers");

            migrationBuilder.AddColumn<string>(
                name: "Wallet",
                table: "Producers",
                maxLength: 255,
                nullable: true);
        }
    }
}
