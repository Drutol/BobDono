using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace BobDono.Migrations
{
    public partial class AddMatchups : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AvatarUrl",
                table: "Users",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Matchup",
                columns: table => new
                {
                    Id = table.Column<long>(type: "int8", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    AuthorId = table.Column<long>(type: "int8", nullable: true),
                    ChallengesEndDate = table.Column<DateTime>(type: "timestamp", nullable: false),
                    CurrentState = table.Column<int>(type: "int4", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    DiscordChannelId = table.Column<long>(type: "int8", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true),
                    OpeningMessageId = table.Column<long>(type: "int8", nullable: false),
                    SignupsEndDate = table.Column<DateTime>(type: "timestamp", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Matchup", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Matchup_Users_AuthorId",
                        column: x => x.AuthorId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MatchupPair",
                columns: table => new
                {
                    Id = table.Column<long>(type: "int8", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    DiscordMessageId = table.Column<long>(type: "int8", nullable: false),
                    FirstId = table.Column<long>(type: "int8", nullable: true),
                    FirstParticipantsChallenge = table.Column<string>(type: "text", nullable: true),
                    FirstParticipantsChallengeCompletionDate = table.Column<DateTime>(type: "timestamp", nullable: false),
                    MatchupId = table.Column<long>(type: "int8", nullable: true),
                    Number = table.Column<int>(type: "int4", nullable: false),
                    SecondId = table.Column<long>(type: "int8", nullable: true),
                    SecondParticipantsChallenge = table.Column<string>(type: "text", nullable: true),
                    SecondParticipantsChallengeCompletionDate = table.Column<DateTime>(type: "timestamp", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MatchupPair", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MatchupPair_Users_FirstId",
                        column: x => x.FirstId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MatchupPair_Matchup_MatchupId",
                        column: x => x.MatchupId,
                        principalTable: "Matchup",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MatchupPair_Users_SecondId",
                        column: x => x.SecondId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserMatchup",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "int8", nullable: false),
                    MatchupId = table.Column<long>(type: "int8", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserMatchup", x => new { x.UserId, x.MatchupId });
                    table.ForeignKey(
                        name: "FK_UserMatchup_Matchup_MatchupId",
                        column: x => x.MatchupId,
                        principalTable: "Matchup",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserMatchup_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Matchup_AuthorId",
                table: "Matchup",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_MatchupPair_FirstId",
                table: "MatchupPair",
                column: "FirstId");

            migrationBuilder.CreateIndex(
                name: "IX_MatchupPair_MatchupId",
                table: "MatchupPair",
                column: "MatchupId");

            migrationBuilder.CreateIndex(
                name: "IX_MatchupPair_SecondId",
                table: "MatchupPair",
                column: "SecondId");

            migrationBuilder.CreateIndex(
                name: "IX_UserMatchup_MatchupId",
                table: "UserMatchup",
                column: "MatchupId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MatchupPair");

            migrationBuilder.DropTable(
                name: "UserMatchup");

            migrationBuilder.DropTable(
                name: "Matchup");

            migrationBuilder.DropColumn(
                name: "AvatarUrl",
                table: "Users");
        }
    }
}
