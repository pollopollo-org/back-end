using Microsoft.EntityFrameworkCore.Migrations;

namespace PolloPollo.Entities.Migrations
{
    public partial class donorpolymorphism : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Country",
                table: "Donors",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                table: "Donors",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SurName",
                table: "Donors",
                maxLength: 255,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Country",
                table: "Donors");

            migrationBuilder.DropColumn(
                name: "FirstName",
                table: "Donors");

            migrationBuilder.DropColumn(
                name: "SurName",
                table: "Donors");
        }
    }
}
