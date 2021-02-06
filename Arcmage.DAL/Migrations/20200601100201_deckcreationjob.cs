using Microsoft.EntityFrameworkCore.Migrations;

namespace Arcmage.DAL.Migrations
{
    public partial class deckcreationjob : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PdfZipCreationJobId",
                table: "DeckModels",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PdfZipCreationJobId",
                table: "DeckModels");
        }
    }
}
