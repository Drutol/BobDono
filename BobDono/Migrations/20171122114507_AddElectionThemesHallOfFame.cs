using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace BobDono.Migrations
{
    public partial class AddElectionThemesHallOfFame : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ElectionThemeChannels",
                columns: table => new
                {
                    Id = table.Column<long>(type: "int8", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    DiscordChannelId = table.Column<long>(type: "int8", nullable: false),
                    NextElection = table.Column<DateTime>(type: "timestamp", nullable: false),
                    OpeningMessageId = table.Column<long>(type: "int8", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ElectionThemeChannels", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ElectionThemes",
                columns: table => new
                {
                    Id = table.Column<long>(type: "int8", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    Approved = table.Column<bool>(type: "bool", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "timestamp", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    DiscordMessageId = table.Column<long>(type: "int8", nullable: false),
                    ElectionCreateDate = table.Column<DateTime>(type: "timestamp", nullable: false),
                    ProposerId = table.Column<long>(type: "int8", nullable: true),
                    Title = table.Column<string>(type: "text", nullable: true),
                    Used = table.Column<bool>(type: "bool", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ElectionThemes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ElectionThemes_Users_ProposerId",
                        column: x => x.ProposerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HallOfFameChannels",
                columns: table => new
                {
                    Id = table.Column<long>(type: "int8", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    DiscordChannelId = table.Column<long>(type: "int8", nullable: false),
                    OpeningMessageId = table.Column<long>(type: "int8", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HallOfFameChannels", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HallOfFameMembers",
                columns: table => new
                {
                    Id = table.Column<long>(type: "int8", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    ContenderId = table.Column<long>(type: "int8", nullable: true),
                    WinDate = table.Column<DateTime>(type: "timestamp", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HallOfFameMembers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HallOfFameMembers_WaifuContenders_ContenderId",
                        column: x => x.ContenderId,
                        principalTable: "WaifuContenders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserTheme",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "int8", nullable: false),
                    ThemeId = table.Column<long>(type: "int8", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserTheme", x => new { x.UserId, x.ThemeId });
                    table.ForeignKey(
                        name: "FK_UserTheme_ElectionThemes_ThemeId",
                        column: x => x.ThemeId,
                        principalTable: "ElectionThemes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserTheme_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ElectionThemes_ProposerId",
                table: "ElectionThemes",
                column: "ProposerId");

            migrationBuilder.CreateIndex(
                name: "IX_HallOfFameMembers_ContenderId",
                table: "HallOfFameMembers",
                column: "ContenderId");

            migrationBuilder.CreateIndex(
                name: "IX_UserTheme_ThemeId",
                table: "UserTheme",
                column: "ThemeId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ElectionThemeChannels");

            migrationBuilder.DropTable(
                name: "HallOfFameChannels");

            migrationBuilder.DropTable(
                name: "HallOfFameMembers");

            migrationBuilder.DropTable(
                name: "UserTheme");

            migrationBuilder.DropTable(
                name: "ElectionThemes");
        }
    }
}
