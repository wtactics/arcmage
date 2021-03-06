using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Arcmage.DAL.Migrations
{
    public partial class rulings : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RulingModels",
                columns: table => new
                {
                    RulingId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CardId = table.Column<int>(type: "int", nullable: true),
                    RuleText = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Guid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatorId = table.Column<int>(type: "int", nullable: false),
                    CreateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedById = table.Column<int>(type: "int", nullable: false),
                    LastModifiedTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RulingModels", x => x.RulingId);
                    table.ForeignKey(
                        name: "FK_RulingModels_CardModels_CardId",
                        column: x => x.CardId,
                        principalTable: "CardModels",
                        principalColumn: "CardId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RulingModels_UserModels_CreatorId",
                        column: x => x.CreatorId,
                        principalTable: "UserModels",
                        principalColumn: "UserId");
                    table.ForeignKey(
                        name: "FK_RulingModels_UserModels_LastModifiedById",
                        column: x => x.LastModifiedById,
                        principalTable: "UserModels",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_RulingModels_CardId",
                table: "RulingModels",
                column: "CardId");

            migrationBuilder.CreateIndex(
                name: "IX_RulingModels_CreatorId",
                table: "RulingModels",
                column: "CreatorId");

            migrationBuilder.CreateIndex(
                name: "IX_RulingModels_LastModifiedById",
                table: "RulingModels",
                column: "LastModifiedById");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RulingModels");
        }
    }
}
