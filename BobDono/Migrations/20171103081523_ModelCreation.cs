using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace BobDono.Migrations
{
    public partial class ModelCreation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Waifus",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    ImageUrl = table.Column<string>(type: "TEXT", nullable: true),
                    MalId = table.Column<string>(type: "TEXT", nullable: true),
                    Name = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Waifus", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TrueWaifus",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    FeatureImage = table.Column<string>(type: "TEXT", nullable: true),
                    WaifuId = table.Column<long>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrueWaifus", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrueWaifus_Waifus_WaifuId",
                        column: x => x.WaifuId,
                        principalTable: "Waifus",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DiscordId = table.Column<ulong>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    TrueWaifuId = table.Column<long>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_TrueWaifus_TrueWaifuId",
                        column: x => x.TrueWaifuId,
                        principalTable: "TrueWaifus",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Elections",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AuthorId = table.Column<long>(type: "INTEGER", nullable: true),
                    BracketMessagesIdsBlob = table.Column<string>(type: "TEXT", nullable: true),
                    CurrentState = table.Column<int>(type: "INTEGER", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    DiscordChannelId = table.Column<ulong>(type: "INTEGER", nullable: false),
                    EntrantsPerUser = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    OpeningMessageId = table.Column<ulong>(type: "INTEGER", nullable: false),
                    PendingVotingStartMessageId = table.Column<ulong>(type: "INTEGER", nullable: false),
                    ResultsMessageId = table.Column<ulong>(type: "INTEGER", nullable: false),
                    StageCount = table.Column<int>(type: "INTEGER", nullable: false),
                    SubmissionsEndDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    SubmissionsStartDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    VotingEndDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    VotingStartDate = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Elections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Elections_Users_AuthorId",
                        column: x => x.AuthorId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserWaifu",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    WaifuId = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserWaifu", x => new { x.UserId, x.WaifuId });
                    table.ForeignKey(
                        name: "FK_UserWaifu_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserWaifu_Waifus_WaifuId",
                        column: x => x.WaifuId,
                        principalTable: "Waifus",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BracketStages",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ElectionId = table.Column<long>(type: "INTEGER", nullable: true),
                    EndDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    StartDate = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BracketStages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BracketStages_Elections_ElectionId",
                        column: x => x.ElectionId,
                        principalTable: "Elections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WaifuContenders",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CustomImageUrl = table.Column<string>(type: "TEXT", nullable: true),
                    ElectionId = table.Column<long>(type: "INTEGER", nullable: true),
                    Lost = table.Column<bool>(type: "INTEGER", nullable: false),
                    ProposerId = table.Column<long>(type: "INTEGER", nullable: true),
                    SeedNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    WaifuId = table.Column<long>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WaifuContenders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WaifuContenders_Elections_ElectionId",
                        column: x => x.ElectionId,
                        principalTable: "Elections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WaifuContenders_Users_ProposerId",
                        column: x => x.ProposerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WaifuContenders_Waifus_WaifuId",
                        column: x => x.WaifuId,
                        principalTable: "Waifus",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Brackets",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    BracketStageId = table.Column<long>(type: "INTEGER", nullable: true),
                    FirstContenderId = table.Column<long>(type: "INTEGER", nullable: true),
                    LoserId = table.Column<long>(type: "INTEGER", nullable: true),
                    Number = table.Column<int>(type: "INTEGER", nullable: false),
                    SecondContenderId = table.Column<long>(type: "INTEGER", nullable: true),
                    ThirdContenderId = table.Column<long>(type: "INTEGER", nullable: true),
                    WinnerId = table.Column<long>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Brackets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Brackets_BracketStages_BracketStageId",
                        column: x => x.BracketStageId,
                        principalTable: "BracketStages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Brackets_WaifuContenders_FirstContenderId",
                        column: x => x.FirstContenderId,
                        principalTable: "WaifuContenders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Brackets_WaifuContenders_LoserId",
                        column: x => x.LoserId,
                        principalTable: "WaifuContenders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Brackets_WaifuContenders_SecondContenderId",
                        column: x => x.SecondContenderId,
                        principalTable: "WaifuContenders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Brackets_WaifuContenders_ThirdContenderId",
                        column: x => x.ThirdContenderId,
                        principalTable: "WaifuContenders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Brackets_WaifuContenders_WinnerId",
                        column: x => x.WinnerId,
                        principalTable: "WaifuContenders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Votes",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    BracketId = table.Column<long>(type: "INTEGER", nullable: true),
                    ContenderId = table.Column<long>(type: "INTEGER", nullable: true),
                    CreateDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UserId = table.Column<long>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Votes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Votes_Brackets_BracketId",
                        column: x => x.BracketId,
                        principalTable: "Brackets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Votes_WaifuContenders_ContenderId",
                        column: x => x.ContenderId,
                        principalTable: "WaifuContenders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Votes_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Brackets_BracketStageId",
                table: "Brackets",
                column: "BracketStageId");

            migrationBuilder.CreateIndex(
                name: "IX_Brackets_FirstContenderId",
                table: "Brackets",
                column: "FirstContenderId");

            migrationBuilder.CreateIndex(
                name: "IX_Brackets_LoserId",
                table: "Brackets",
                column: "LoserId");

            migrationBuilder.CreateIndex(
                name: "IX_Brackets_SecondContenderId",
                table: "Brackets",
                column: "SecondContenderId");

            migrationBuilder.CreateIndex(
                name: "IX_Brackets_ThirdContenderId",
                table: "Brackets",
                column: "ThirdContenderId");

            migrationBuilder.CreateIndex(
                name: "IX_Brackets_WinnerId",
                table: "Brackets",
                column: "WinnerId");

            migrationBuilder.CreateIndex(
                name: "IX_BracketStages_ElectionId",
                table: "BracketStages",
                column: "ElectionId");

            migrationBuilder.CreateIndex(
                name: "IX_Elections_AuthorId",
                table: "Elections",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_TrueWaifus_WaifuId",
                table: "TrueWaifus",
                column: "WaifuId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_TrueWaifuId",
                table: "Users",
                column: "TrueWaifuId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserWaifu_WaifuId",
                table: "UserWaifu",
                column: "WaifuId");

            migrationBuilder.CreateIndex(
                name: "IX_Votes_BracketId",
                table: "Votes",
                column: "BracketId");

            migrationBuilder.CreateIndex(
                name: "IX_Votes_ContenderId",
                table: "Votes",
                column: "ContenderId");

            migrationBuilder.CreateIndex(
                name: "IX_Votes_UserId",
                table: "Votes",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_WaifuContenders_ElectionId",
                table: "WaifuContenders",
                column: "ElectionId");

            migrationBuilder.CreateIndex(
                name: "IX_WaifuContenders_ProposerId",
                table: "WaifuContenders",
                column: "ProposerId");

            migrationBuilder.CreateIndex(
                name: "IX_WaifuContenders_WaifuId",
                table: "WaifuContenders",
                column: "WaifuId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserWaifu");

            migrationBuilder.DropTable(
                name: "Votes");

            migrationBuilder.DropTable(
                name: "Brackets");

            migrationBuilder.DropTable(
                name: "BracketStages");

            migrationBuilder.DropTable(
                name: "WaifuContenders");

            migrationBuilder.DropTable(
                name: "Elections");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "TrueWaifus");

            migrationBuilder.DropTable(
                name: "Waifus");
        }
    }
}
