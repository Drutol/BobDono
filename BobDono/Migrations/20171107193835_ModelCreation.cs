using Microsoft.EntityFrameworkCore.Metadata;
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
                    Id = table.Column<long>(type: "int8", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    Description = table.Column<string>(type: "text", nullable: true),
                    ImageUrl = table.Column<string>(type: "text", nullable: true),
                    MalId = table.Column<string>(type: "text", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Waifus", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TrueWaifus",
                columns: table => new
                {
                    Id = table.Column<long>(type: "int8", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    Description = table.Column<string>(type: "text", nullable: true),
                    FeatureImage = table.Column<string>(type: "text", nullable: true),
                    WaifuId = table.Column<long>(type: "int8", nullable: true)
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
                    Id = table.Column<long>(type: "int8", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    Name = table.Column<string>(type: "text", nullable: true),
                    TrueWaifuId = table.Column<long>(type: "int8", nullable: true),
                    _discordId = table.Column<long>(type: "int8", nullable: false)
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
                    Id = table.Column<long>(type: "int8", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    AuthorId = table.Column<long>(type: "int8", nullable: true),
                    BracketMessagesIdsBlob = table.Column<string>(type: "text", nullable: true),
                    CurrentState = table.Column<int>(type: "int4", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    EntrantsPerUser = table.Column<int>(type: "int4", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true),
                    StageCount = table.Column<int>(type: "int4", nullable: false),
                    SubmissionsEndDate = table.Column<DateTime>(type: "timestamp", nullable: false),
                    SubmissionsStartDate = table.Column<DateTime>(type: "timestamp", nullable: false),
                    VotingEndDate = table.Column<DateTime>(type: "timestamp", nullable: false),
                    VotingStartDate = table.Column<DateTime>(type: "timestamp", nullable: false),
                    _discordChannelId = table.Column<long>(type: "int8", nullable: false),
                    _openingMessageId = table.Column<long>(type: "int8", nullable: false),
                    _pendingVotingStartMessageId = table.Column<long>(type: "int8", nullable: false),
                    _resultsMessageId = table.Column<long>(type: "int8", nullable: false)
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
                    UserId = table.Column<long>(type: "int8", nullable: false),
                    WaifuId = table.Column<long>(type: "int8", nullable: false)
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
                    Id = table.Column<long>(type: "int8", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    ElectionId = table.Column<long>(type: "int8", nullable: true),
                    EndDate = table.Column<DateTime>(type: "timestamp", nullable: false),
                    Number = table.Column<int>(type: "int4", nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp", nullable: false)
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
                    Id = table.Column<long>(type: "int8", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    CustomImageUrl = table.Column<string>(type: "text", nullable: true),
                    ElectionId = table.Column<long>(type: "int8", nullable: true),
                    FeatureImage = table.Column<string>(type: "text", nullable: true),
                    Lost = table.Column<bool>(type: "bool", nullable: false),
                    ProposerId = table.Column<long>(type: "int8", nullable: true),
                    SeedNumber = table.Column<int>(type: "int4", nullable: false),
                    WaifuId = table.Column<long>(type: "int8", nullable: true)
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
                    Id = table.Column<long>(type: "int8", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    BracketStageId = table.Column<long>(type: "int8", nullable: true),
                    FirstContenderId = table.Column<long>(type: "int8", nullable: true),
                    LoserId = table.Column<long>(type: "int8", nullable: true),
                    Number = table.Column<int>(type: "int4", nullable: false),
                    SecondContenderId = table.Column<long>(type: "int8", nullable: true),
                    ThirdContenderId = table.Column<long>(type: "int8", nullable: true),
                    WinnerId = table.Column<long>(type: "int8", nullable: true)
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
                    Id = table.Column<long>(type: "int8", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    BracketId = table.Column<long>(type: "int8", nullable: true),
                    ContenderId = table.Column<long>(type: "int8", nullable: true),
                    CreateDate = table.Column<DateTime>(type: "timestamp", nullable: false),
                    UserId = table.Column<long>(type: "int8", nullable: true)
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
