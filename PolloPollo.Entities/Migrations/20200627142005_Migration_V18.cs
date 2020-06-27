using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace PolloPollo.Entities.Migrations
{
    public partial class Migration_V18 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "DonationDate",
                table: "Applications",
                nullable: true,
                oldClrType: typeof(DateTime));

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
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);
        }
    }
}
