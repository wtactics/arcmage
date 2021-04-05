using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Arcmage.DAL.Migrations
{
    public partial class card_artwork_license : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ArtworkLicenseLicenseId",
                table: "CardModels",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ArtworkLicensor",
                table: "CardModels",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "LicenseModels",
                columns: table => new
                {
                    LicenseId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Url = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Guid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatorId = table.Column<int>(type: "int", nullable: false),
                    CreateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedById = table.Column<int>(type: "int", nullable: false),
                    LastModifiedTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LicenseModels", x => x.LicenseId);
                    table.ForeignKey(
                        name: "FK_LicenseModels_UserModels_CreatorId",
                        column: x => x.CreatorId,
                        principalTable: "UserModels",
                        principalColumn: "UserId");
                    table.ForeignKey(
                        name: "FK_LicenseModels_UserModels_LastModifiedById",
                        column: x => x.LastModifiedById,
                        principalTable: "UserModels",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_CardModels_ArtworkLicenseLicenseId",
                table: "CardModels",
                column: "ArtworkLicenseLicenseId");

            migrationBuilder.CreateIndex(
                name: "IX_LicenseModels_CreatorId",
                table: "LicenseModels",
                column: "CreatorId");

            migrationBuilder.CreateIndex(
                name: "IX_LicenseModels_LastModifiedById",
                table: "LicenseModels",
                column: "LastModifiedById");

            migrationBuilder.AddForeignKey(
                name: "FK_CardModels_LicenseModels_ArtworkLicenseLicenseId",
                table: "CardModels",
                column: "ArtworkLicenseLicenseId",
                principalTable: "LicenseModels",
                principalColumn: "LicenseId",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CardModels_LicenseModels_ArtworkLicenseLicenseId",
                table: "CardModels");

            migrationBuilder.DropTable(
                name: "LicenseModels");

            migrationBuilder.DropIndex(
                name: "IX_CardModels_ArtworkLicenseLicenseId",
                table: "CardModels");

            migrationBuilder.DropColumn(
                name: "ArtworkLicenseLicenseId",
                table: "CardModels");

            migrationBuilder.DropColumn(
                name: "ArtworkLicensor",
                table: "CardModels");
        }
    }
}
