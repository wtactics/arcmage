using Microsoft.EntityFrameworkCore.Migrations;

namespace Arcmage.DAL.Migrations
{
    public partial class deck_status : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "StatusId",
                table: "DeckModels",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DeckModels_StatusId",
                table: "DeckModels",
                column: "StatusId");

            migrationBuilder.AddForeignKey(
                name: "FK_DeckModels_StatusModels_StatusId",
                table: "DeckModels",
                column: "StatusId",
                principalTable: "StatusModels",
                principalColumn: "StatusId",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DeckModels_StatusModels_StatusId",
                table: "DeckModels");

            migrationBuilder.DropIndex(
                name: "IX_DeckModels_StatusId",
                table: "DeckModels");

            migrationBuilder.DropColumn(
                name: "StatusId",
                table: "DeckModels");
        }
    }
}
