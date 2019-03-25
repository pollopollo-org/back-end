using Microsoft.EntityFrameworkCore.Migrations;

namespace PolloPollo.Entities.Migrations
{
    public partial class Migration_V2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Surname",
                table: "Users",
                newName: "SurName");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SurName",
                table: "Users",
                newName: "Surname");
        }
    }
}
