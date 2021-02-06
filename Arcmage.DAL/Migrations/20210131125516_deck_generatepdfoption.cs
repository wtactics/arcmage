using Microsoft.EntityFrameworkCore.Migrations;

namespace Arcmage.DAL.Migrations
{
    public partial class deck_generatepdfoption : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "GeneratePdf",
                table: "DeckModels",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GeneratePdf",
                table: "DeckModels");
        }
    }
}
