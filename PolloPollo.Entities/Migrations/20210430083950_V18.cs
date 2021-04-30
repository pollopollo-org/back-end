using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace PolloPollo.Entities.Migrations
{
    public partial class V18 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Applications_Users_UserId",
                table: "Applications");

            migrationBuilder.AlterColumn<string>(
                name: "DonationDate",
                table: "Applications",
                nullable: true,
                oldClrType: typeof(DateTime));

            migrationBuilder.AddColumn<string>(
                name: "UnitId",
                table: "Applications",
                maxLength: 44,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ByteExchangeRate",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    GBYTE_USD = table.Column<decimal>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ByteExchangeRate", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Contracts",
                columns: table => new
                {
                    ApplicationId = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CreationTime = table.Column<DateTime>(nullable: true),
                    Completed = table.Column<int>(nullable: true),
                    ConfirmKey = table.Column<string>(nullable: true),
                    SharedAddress = table.Column<string>(nullable: true),
                    DonorWallet = table.Column<string>(nullable: true),
                    DonorDevice = table.Column<string>(nullable: true),
                    ProducerWallet = table.Column<string>(nullable: true),
                    ProducerDevice = table.Column<string>(nullable: true),
                    Price = table.Column<int>(nullable: true),
                    Bytes = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contracts", x => x.ApplicationId);
                });

            migrationBuilder.CreateTable(
                name: "Donors",
                columns: table => new
                {
                    AaAccount = table.Column<string>(maxLength: 128, nullable: false),
                    UID = table.Column<string>(maxLength: 128, nullable: true),
                    Password = table.Column<string>(maxLength: 256, nullable: true),
                    Email = table.Column<string>(maxLength: 256, nullable: true),
                    DeviceAddress = table.Column<string>(maxLength: 34, nullable: true),
                    WalletAddress = table.Column<string>(maxLength: 34, nullable: true),
                    Thumbnail = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Donors", x => x.AaAccount);
                });

            migrationBuilder.CreateTable(
                name: "Newsletter",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    DeviceAddress = table.Column<string>(maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Newsletter", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Newsletter_DeviceAddress",
                table: "Newsletter",
                column: "DeviceAddress",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Applications_Users_UserId",
                table: "Applications",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Applications_Users_UserId",
                table: "Applications");

            migrationBuilder.DropTable(
                name: "ByteExchangeRate");

            migrationBuilder.DropTable(
                name: "Contracts");

            migrationBuilder.DropTable(
                name: "Donors");

            migrationBuilder.DropTable(
                name: "Newsletter");

            migrationBuilder.DropColumn(
                name: "UnitId",
                table: "Applications");

            migrationBuilder.AlterColumn<DateTime>(
                name: "DonationDate",
                table: "Applications",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Applications_Users_UserId",
                table: "Applications",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
