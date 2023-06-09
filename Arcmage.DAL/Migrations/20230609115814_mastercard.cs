using Microsoft.EntityFrameworkCore.Migrations;

namespace Arcmage.DAL.Migrations
{
    public partial class mastercard : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MasterCardId",
                table: "CardModels",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CardModels_MasterCardId",
                table: "CardModels",
                column: "MasterCardId");

            migrationBuilder.AddForeignKey(
                name: "FK_CardModels_CardModels_MasterCardId",
                table: "CardModels",
                column: "MasterCardId",
                principalTable: "CardModels",
                principalColumn: "CardId",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CardModels_CardModels_MasterCardId",
                table: "CardModels");

            migrationBuilder.DropIndex(
                name: "IX_CardModels_MasterCardId",
                table: "CardModels");

            migrationBuilder.DropColumn(
                name: "MasterCardId",
                table: "CardModels");
        }
    }
}
