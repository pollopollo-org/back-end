using Microsoft.EntityFrameworkCore.Migrations;

namespace PolloPollo.Entities.Migrations
{
    public partial class Migration_V19 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "WalletAddress",
                table: "Donors",
                maxLength: 34,
                nullable: false,
                oldClrType: typeof(string),
                oldMaxLength: 34,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Password",
                table: "Donors",
                maxLength: 64,
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 64);

            migrationBuilder.AddColumn<string>(
                name: "UnitId",
                table: "Applications",
                maxLength: 44,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UnitId",
                table: "Applications");

            migrationBuilder.AlterColumn<string>(
                name: "WalletAddress",
                table: "Donors",
                maxLength: 34,
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 34);

            migrationBuilder.AlterColumn<string>(
                name: "Password",
                table: "Donors",
                maxLength: 64,
                nullable: false,
                oldClrType: typeof(string),
                oldMaxLength: 64,
                oldNullable: true);
        }
    }
}
