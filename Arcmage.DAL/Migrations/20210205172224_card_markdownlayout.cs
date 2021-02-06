using Microsoft.EntityFrameworkCore.Migrations;

namespace Arcmage.DAL.Migrations
{
    public partial class card_markdownlayout : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MarkdownText",
                table: "CardModels",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MarkdownText",
                table: "CardModels");
        }
    }
}
