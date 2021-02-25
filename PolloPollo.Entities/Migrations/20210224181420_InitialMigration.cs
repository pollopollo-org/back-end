using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace PolloPollo.Entities.Migrations
{
    public partial class InitialMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "DonationDate",
                table: "Applications",
                type: "longtext CHARACTER SET utf8mb4",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)");

            migrationBuilder.CreateTable(
                name: "ByteExchangeRate",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    GBYTE_USD = table.Column<decimal>(type: "decimal(65,30)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ByteExchangeRate", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Contracts",
                columns: table => new
                {
                    ApplicationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CreationTime = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Completed = table.Column<int>(type: "int", nullable: true),
                    ConfirmKey = table.Column<string>(type: "longtext CHARACTER SET utf8mb4", nullable: true),
                    SharedAddress = table.Column<string>(type: "longtext CHARACTER SET utf8mb4", nullable: true),
                    DonorWallet = table.Column<string>(type: "longtext CHARACTER SET utf8mb4", nullable: true),
                    DonorDevice = table.Column<string>(type: "longtext CHARACTER SET utf8mb4", nullable: true),
                    ProducerWallet = table.Column<string>(type: "longtext CHARACTER SET utf8mb4", nullable: true),
                    ProducerDevice = table.Column<string>(type: "longtext CHARACTER SET utf8mb4", nullable: true),
                    Price = table.Column<int>(type: "int", nullable: true),
                    Bytes = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contracts", x => x.ApplicationId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ByteExchangeRate");

            migrationBuilder.DropTable(
                name: "Contracts");

            migrationBuilder.AlterColumn<DateTime>(
                name: "DonationDate",
                table: "Applications",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(string),
                oldType: "longtext CHARACTER SET utf8mb4",
                oldNullable: true);
        }
    }
}
