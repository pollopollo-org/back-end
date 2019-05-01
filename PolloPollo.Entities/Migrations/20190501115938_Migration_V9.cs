using System;
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

            migrationBuilder.RenameColumn(
                name: "TimeStamp",
                table: "Products",
                newName: "Created");

            migrationBuilder.RenameColumn(
                name: "TimeStamp",
                table: "Applications",
                newName: "LastModified");

            migrationBuilder.AddColumn<DateTime>(
                name: "Created",
                table: "Users",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "DeviceAddress",
                table: "Producers",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PairingSecret",
                table: "Producers",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "WalletAddress",
                table: "Producers",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Created",
                table: "Applications",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Created",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "DeviceAddress",
                table: "Producers");

            migrationBuilder.DropColumn(
                name: "PairingSecret",
                table: "Producers");

            migrationBuilder.DropColumn(
                name: "WalletAddress",
                table: "Producers");

            migrationBuilder.DropColumn(
                name: "Created",
                table: "Applications");

            migrationBuilder.RenameColumn(
                name: "Created",
                table: "Products",
                newName: "TimeStamp");

            migrationBuilder.RenameColumn(
                name: "LastModified",
                table: "Applications",
                newName: "TimeStamp");

            migrationBuilder.AddColumn<string>(
                name: "Wallet",
                table: "Producers",
                maxLength: 255,
                nullable: true);
        }
    }
}
