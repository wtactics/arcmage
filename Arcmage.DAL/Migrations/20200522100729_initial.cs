using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Arcmage.DAL.Migrations
{
    public partial class initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DeckCardModels",
                columns: table => new
                {
                    DeckCardId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Guid = table.Column<Guid>(nullable: false),
                    CreatorId = table.Column<int>(nullable: false),
                    CreateTime = table.Column<DateTime>(nullable: false),
                    LastModifiedById = table.Column<int>(nullable: false),
                    LastModifiedTime = table.Column<DateTime>(nullable: false),
                    Quantity = table.Column<int>(nullable: false),
                    DeckId = table.Column<int>(nullable: true),
                    CardId = table.Column<int>(nullable: true),
                    PdfCreationJobId = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeckCardModels", x => x.DeckCardId);
                });

            migrationBuilder.CreateTable(
                name: "CardModels",
                columns: table => new
                {
                    CardId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Guid = table.Column<Guid>(nullable: false),
                    CreatorId = table.Column<int>(nullable: false),
                    CreateTime = table.Column<DateTime>(nullable: false),
                    LastModifiedById = table.Column<int>(nullable: false),
                    LastModifiedTime = table.Column<DateTime>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    FirstName = table.Column<string>(nullable: true),
                    LastName = table.Column<string>(nullable: true),
                    Artist = table.Column<string>(nullable: true),
                    RuleText = table.Column<string>(nullable: true),
                    FlavorText = table.Column<string>(nullable: true),
                    SubType = table.Column<string>(nullable: true),
                    TypeCardTypeId = table.Column<int>(nullable: true),
                    FactionId = table.Column<int>(nullable: true),
                    Cost = table.Column<string>(nullable: true),
                    Loyalty = table.Column<int>(nullable: false),
                    Attack = table.Column<string>(nullable: true),
                    Defense = table.Column<string>(nullable: true),
                    Info = table.Column<string>(nullable: true),
                    SerieId = table.Column<int>(nullable: true),
                    RuleSetId = table.Column<int>(nullable: true),
                    StatusId = table.Column<int>(nullable: true),
                    LayoutText = table.Column<string>(nullable: true),
                    PngCreationJobId = table.Column<string>(nullable: true),
                    UserModelUserId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CardModels", x => x.CardId);
                });

            migrationBuilder.CreateTable(
                name: "UserModels",
                columns: table => new
                {
                    UserId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Guid = table.Column<Guid>(nullable: false),
                    CreateTime = table.Column<DateTime>(nullable: false),
                    LastLoginTime = table.Column<DateTime>(nullable: false),
                    Token = table.Column<string>(nullable: true),
                    Name = table.Column<string>(nullable: true),
                    Email = table.Column<string>(nullable: true),
                    Password = table.Column<string>(nullable: true),
                    RoleId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserModels", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "DeckModels",
                columns: table => new
                {
                    DeckId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Guid = table.Column<Guid>(nullable: false),
                    CreatorId = table.Column<int>(nullable: false),
                    CreateTime = table.Column<DateTime>(nullable: false),
                    LastModifiedById = table.Column<int>(nullable: false),
                    LastModifiedTime = table.Column<DateTime>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    Pdf = table.Column<string>(nullable: true),
                    ExportTiles = table.Column<bool>(nullable: false),
                    UserModelUserId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeckModels", x => x.DeckId);
                    table.ForeignKey(
                        name: "FK_DeckModels_UserModels_CreatorId",
                        column: x => x.CreatorId,
                        principalTable: "UserModels",
                        principalColumn: "UserId");
                    table.ForeignKey(
                        name: "FK_DeckModels_UserModels_LastModifiedById",
                        column: x => x.LastModifiedById,
                        principalTable: "UserModels",
                        principalColumn: "UserId");
                    table.ForeignKey(
                        name: "FK_DeckModels_UserModels_UserModelUserId",
                        column: x => x.UserModelUserId,
                        principalTable: "UserModels",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FactionModels",
                columns: table => new
                {
                    FactionId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Guid = table.Column<Guid>(nullable: false),
                    CreatorId = table.Column<int>(nullable: false),
                    CreateTime = table.Column<DateTime>(nullable: false),
                    LastModifiedById = table.Column<int>(nullable: false),
                    LastModifiedTime = table.Column<DateTime>(nullable: false),
                    Name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FactionModels", x => x.FactionId);
                    table.ForeignKey(
                        name: "FK_FactionModels_UserModels_CreatorId",
                        column: x => x.CreatorId,
                        principalTable: "UserModels",
                        principalColumn: "UserId");
                    table.ForeignKey(
                        name: "FK_FactionModels_UserModels_LastModifiedById",
                        column: x => x.LastModifiedById,
                        principalTable: "UserModels",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "RoleModels",
                columns: table => new
                {
                    RoleId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Guid = table.Column<Guid>(nullable: false),
                    CreatorId = table.Column<int>(nullable: false),
                    CreateTime = table.Column<DateTime>(nullable: false),
                    LastModifiedById = table.Column<int>(nullable: false),
                    LastModifiedTime = table.Column<DateTime>(nullable: false),
                    Name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleModels", x => x.RoleId);
                    table.ForeignKey(
                        name: "FK_RoleModels_UserModels_CreatorId",
                        column: x => x.CreatorId,
                        principalTable: "UserModels",
                        principalColumn: "UserId");
                    table.ForeignKey(
                        name: "FK_RoleModels_UserModels_LastModifiedById",
                        column: x => x.LastModifiedById,
                        principalTable: "UserModels",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "StatusModels",
                columns: table => new
                {
                    StatusId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Guid = table.Column<Guid>(nullable: false),
                    CreatorId = table.Column<int>(nullable: false),
                    CreateTime = table.Column<DateTime>(nullable: false),
                    LastModifiedById = table.Column<int>(nullable: false),
                    LastModifiedTime = table.Column<DateTime>(nullable: false),
                    Name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StatusModels", x => x.StatusId);
                    table.ForeignKey(
                        name: "FK_StatusModels_UserModels_CreatorId",
                        column: x => x.CreatorId,
                        principalTable: "UserModels",
                        principalColumn: "UserId");
                    table.ForeignKey(
                        name: "FK_StatusModels_UserModels_LastModifiedById",
                        column: x => x.LastModifiedById,
                        principalTable: "UserModels",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "TemplateInfoModels",
                columns: table => new
                {
                    TemplateInfoId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Guid = table.Column<Guid>(nullable: false),
                    CreatorId = table.Column<int>(nullable: false),
                    CreateTime = table.Column<DateTime>(nullable: false),
                    LastModifiedById = table.Column<int>(nullable: false),
                    LastModifiedTime = table.Column<DateTime>(nullable: false),
                    ShowName = table.Column<bool>(nullable: false),
                    ShowType = table.Column<bool>(nullable: false),
                    ShowFaction = table.Column<bool>(nullable: false),
                    ShowGoldCost = table.Column<bool>(nullable: false),
                    ShowLoyalty = table.Column<bool>(nullable: false),
                    ShowText = table.Column<bool>(nullable: false),
                    ShowAttack = table.Column<bool>(nullable: false),
                    ShowDefense = table.Column<bool>(nullable: false),
                    ShowDiscipline = table.Column<bool>(nullable: false),
                    ShowArt = table.Column<bool>(nullable: false),
                    ShowInfo = table.Column<bool>(nullable: false),
                    MaxTextBoxWidth = table.Column<double>(nullable: false),
                    MaxTextBoxHeight = table.Column<double>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TemplateInfoModels", x => x.TemplateInfoId);
                    table.ForeignKey(
                        name: "FK_TemplateInfoModels_UserModels_CreatorId",
                        column: x => x.CreatorId,
                        principalTable: "UserModels",
                        principalColumn: "UserId");
                    table.ForeignKey(
                        name: "FK_TemplateInfoModels_UserModels_LastModifiedById",
                        column: x => x.LastModifiedById,
                        principalTable: "UserModels",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "RuleSetModels",
                columns: table => new
                {
                    RuleSetId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Guid = table.Column<Guid>(nullable: false),
                    CreatorId = table.Column<int>(nullable: false),
                    CreateTime = table.Column<DateTime>(nullable: false),
                    LastModifiedById = table.Column<int>(nullable: false),
                    LastModifiedTime = table.Column<DateTime>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    StatusId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RuleSetModels", x => x.RuleSetId);
                    table.ForeignKey(
                        name: "FK_RuleSetModels_UserModels_CreatorId",
                        column: x => x.CreatorId,
                        principalTable: "UserModels",
                        principalColumn: "UserId");
                    table.ForeignKey(
                        name: "FK_RuleSetModels_UserModels_LastModifiedById",
                        column: x => x.LastModifiedById,
                        principalTable: "UserModels",
                        principalColumn: "UserId");
                    table.ForeignKey(
                        name: "FK_RuleSetModels_StatusModels_StatusId",
                        column: x => x.StatusId,
                        principalTable: "StatusModels",
                        principalColumn: "StatusId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SerieModels",
                columns: table => new
                {
                    SerieId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Guid = table.Column<Guid>(nullable: false),
                    CreatorId = table.Column<int>(nullable: false),
                    CreateTime = table.Column<DateTime>(nullable: false),
                    LastModifiedById = table.Column<int>(nullable: false),
                    LastModifiedTime = table.Column<DateTime>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    StatusId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SerieModels", x => x.SerieId);
                    table.ForeignKey(
                        name: "FK_SerieModels_UserModels_CreatorId",
                        column: x => x.CreatorId,
                        principalTable: "UserModels",
                        principalColumn: "UserId");
                    table.ForeignKey(
                        name: "FK_SerieModels_UserModels_LastModifiedById",
                        column: x => x.LastModifiedById,
                        principalTable: "UserModels",
                        principalColumn: "UserId");
                    table.ForeignKey(
                        name: "FK_SerieModels_StatusModels_StatusId",
                        column: x => x.StatusId,
                        principalTable: "StatusModels",
                        principalColumn: "StatusId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CardTypeModels",
                columns: table => new
                {
                    CardTypeId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Guid = table.Column<Guid>(nullable: false),
                    CreatorId = table.Column<int>(nullable: false),
                    CreateTime = table.Column<DateTime>(nullable: false),
                    LastModifiedById = table.Column<int>(nullable: false),
                    LastModifiedTime = table.Column<DateTime>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    TemplateInfoId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CardTypeModels", x => x.CardTypeId);
                    table.ForeignKey(
                        name: "FK_CardTypeModels_UserModels_CreatorId",
                        column: x => x.CreatorId,
                        principalTable: "UserModels",
                        principalColumn: "UserId");
                    table.ForeignKey(
                        name: "FK_CardTypeModels_UserModels_LastModifiedById",
                        column: x => x.LastModifiedById,
                        principalTable: "UserModels",
                        principalColumn: "UserId");
                    table.ForeignKey(
                        name: "FK_CardTypeModels_TemplateInfoModels_TemplateInfoId",
                        column: x => x.TemplateInfoId,
                        principalTable: "TemplateInfoModels",
                        principalColumn: "TemplateInfoId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CardModels_CreatorId",
                table: "CardModels",
                column: "CreatorId");

            migrationBuilder.CreateIndex(
                name: "IX_CardModels_FactionId",
                table: "CardModels",
                column: "FactionId");

            migrationBuilder.CreateIndex(
                name: "IX_CardModels_LastModifiedById",
                table: "CardModels",
                column: "LastModifiedById");

            migrationBuilder.CreateIndex(
                name: "IX_CardModels_RuleSetId",
                table: "CardModels",
                column: "RuleSetId");

            migrationBuilder.CreateIndex(
                name: "IX_CardModels_SerieId",
                table: "CardModels",
                column: "SerieId");

            migrationBuilder.CreateIndex(
                name: "IX_CardModels_StatusId",
                table: "CardModels",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_CardModels_TypeCardTypeId",
                table: "CardModels",
                column: "TypeCardTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_CardModels_UserModelUserId",
                table: "CardModels",
                column: "UserModelUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CardTypeModels_CreatorId",
                table: "CardTypeModels",
                column: "CreatorId");

            migrationBuilder.CreateIndex(
                name: "IX_CardTypeModels_LastModifiedById",
                table: "CardTypeModels",
                column: "LastModifiedById");

            migrationBuilder.CreateIndex(
                name: "IX_CardTypeModels_TemplateInfoId",
                table: "CardTypeModels",
                column: "TemplateInfoId");

            migrationBuilder.CreateIndex(
                name: "IX_DeckCardModels_CardId",
                table: "DeckCardModels",
                column: "CardId");

            migrationBuilder.CreateIndex(
                name: "IX_DeckCardModels_CreatorId",
                table: "DeckCardModels",
                column: "CreatorId");

            migrationBuilder.CreateIndex(
                name: "IX_DeckCardModels_DeckId",
                table: "DeckCardModels",
                column: "DeckId");

            migrationBuilder.CreateIndex(
                name: "IX_DeckCardModels_LastModifiedById",
                table: "DeckCardModels",
                column: "LastModifiedById");

            migrationBuilder.CreateIndex(
                name: "IX_DeckModels_CreatorId",
                table: "DeckModels",
                column: "CreatorId");

            migrationBuilder.CreateIndex(
                name: "IX_DeckModels_LastModifiedById",
                table: "DeckModels",
                column: "LastModifiedById");

            migrationBuilder.CreateIndex(
                name: "IX_DeckModels_UserModelUserId",
                table: "DeckModels",
                column: "UserModelUserId");

            migrationBuilder.CreateIndex(
                name: "IX_FactionModels_CreatorId",
                table: "FactionModels",
                column: "CreatorId");

            migrationBuilder.CreateIndex(
                name: "IX_FactionModels_LastModifiedById",
                table: "FactionModels",
                column: "LastModifiedById");

            migrationBuilder.CreateIndex(
                name: "IX_RoleModels_CreatorId",
                table: "RoleModels",
                column: "CreatorId");

            migrationBuilder.CreateIndex(
                name: "IX_RoleModels_LastModifiedById",
                table: "RoleModels",
                column: "LastModifiedById");

            migrationBuilder.CreateIndex(
                name: "IX_RuleSetModels_CreatorId",
                table: "RuleSetModels",
                column: "CreatorId");

            migrationBuilder.CreateIndex(
                name: "IX_RuleSetModels_LastModifiedById",
                table: "RuleSetModels",
                column: "LastModifiedById");

            migrationBuilder.CreateIndex(
                name: "IX_RuleSetModels_StatusId",
                table: "RuleSetModels",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_SerieModels_CreatorId",
                table: "SerieModels",
                column: "CreatorId");

            migrationBuilder.CreateIndex(
                name: "IX_SerieModels_LastModifiedById",
                table: "SerieModels",
                column: "LastModifiedById");

            migrationBuilder.CreateIndex(
                name: "IX_SerieModels_StatusId",
                table: "SerieModels",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_StatusModels_CreatorId",
                table: "StatusModels",
                column: "CreatorId");

            migrationBuilder.CreateIndex(
                name: "IX_StatusModels_LastModifiedById",
                table: "StatusModels",
                column: "LastModifiedById");

            migrationBuilder.CreateIndex(
                name: "IX_TemplateInfoModels_CreatorId",
                table: "TemplateInfoModels",
                column: "CreatorId");

            migrationBuilder.CreateIndex(
                name: "IX_TemplateInfoModels_LastModifiedById",
                table: "TemplateInfoModels",
                column: "LastModifiedById");

            migrationBuilder.CreateIndex(
                name: "IX_UserModels_RoleId",
                table: "UserModels",
                column: "RoleId");

            migrationBuilder.AddForeignKey(
                name: "FK_DeckCardModels_UserModels_CreatorId",
                table: "DeckCardModels",
                column: "CreatorId",
                principalTable: "UserModels",
                principalColumn: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_DeckCardModels_UserModels_LastModifiedById",
                table: "DeckCardModels",
                column: "LastModifiedById",
                principalTable: "UserModels",
                principalColumn: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_DeckCardModels_CardModels_CardId",
                table: "DeckCardModels",
                column: "CardId",
                principalTable: "CardModels",
                principalColumn: "CardId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DeckCardModels_DeckModels_DeckId",
                table: "DeckCardModels",
                column: "DeckId",
                principalTable: "DeckModels",
                principalColumn: "DeckId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CardModels_UserModels_CreatorId",
                table: "CardModels",
                column: "CreatorId",
                principalTable: "UserModels",
                principalColumn: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_CardModels_UserModels_LastModifiedById",
                table: "CardModels",
                column: "LastModifiedById",
                principalTable: "UserModels",
                principalColumn: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_CardModels_UserModels_UserModelUserId",
                table: "CardModels",
                column: "UserModelUserId",
                principalTable: "UserModels",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CardModels_FactionModels_FactionId",
                table: "CardModels",
                column: "FactionId",
                principalTable: "FactionModels",
                principalColumn: "FactionId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CardModels_RuleSetModels_RuleSetId",
                table: "CardModels",
                column: "RuleSetId",
                principalTable: "RuleSetModels",
                principalColumn: "RuleSetId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CardModels_SerieModels_SerieId",
                table: "CardModels",
                column: "SerieId",
                principalTable: "SerieModels",
                principalColumn: "SerieId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CardModels_StatusModels_StatusId",
                table: "CardModels",
                column: "StatusId",
                principalTable: "StatusModels",
                principalColumn: "StatusId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CardModels_CardTypeModels_TypeCardTypeId",
                table: "CardModels",
                column: "TypeCardTypeId",
                principalTable: "CardTypeModels",
                principalColumn: "CardTypeId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserModels_RoleModels_RoleId",
                table: "UserModels",
                column: "RoleId",
                principalTable: "RoleModels",
                principalColumn: "RoleId",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RoleModels_UserModels_CreatorId",
                table: "RoleModels");

            migrationBuilder.DropForeignKey(
                name: "FK_RoleModels_UserModels_LastModifiedById",
                table: "RoleModels");

            migrationBuilder.DropTable(
                name: "DeckCardModels");

            migrationBuilder.DropTable(
                name: "CardModels");

            migrationBuilder.DropTable(
                name: "DeckModels");

            migrationBuilder.DropTable(
                name: "FactionModels");

            migrationBuilder.DropTable(
                name: "RuleSetModels");

            migrationBuilder.DropTable(
                name: "SerieModels");

            migrationBuilder.DropTable(
                name: "CardTypeModels");

            migrationBuilder.DropTable(
                name: "StatusModels");

            migrationBuilder.DropTable(
                name: "TemplateInfoModels");

            migrationBuilder.DropTable(
                name: "UserModels");

            migrationBuilder.DropTable(
                name: "RoleModels");
        }
    }
}
