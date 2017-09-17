using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace BobDono.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<ulong>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Elections",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AuthorId = table.Column<ulong>(type: "INTEGER", nullable: true),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    EndDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    StartDate = table.Column<DateTime>(type: "TEXT", nullable: false)
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
                name: "Waifus",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    ImageUrl = table.Column<string>(type: "TEXT", nullable: true),
                    MalId = table.Column<string>(type: "TEXT", nullable: true),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    UserId = table.Column<ulong>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Waifus", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Waifus_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WaifuContenders",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ElectionId = table.Column<long>(type: "INTEGER", nullable: true),
                    ProposerId = table.Column<ulong>(type: "INTEGER", nullable: true),
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
                    ElectionId = table.Column<long>(type: "INTEGER", nullable: true),
                    EndDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    FirstWaifuId = table.Column<long>(type: "INTEGER", nullable: true),
                    SecondWaifuId = table.Column<long>(type: "INTEGER", nullable: true),
                    StartDate = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Brackets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Brackets_Elections_ElectionId",
                        column: x => x.ElectionId,
                        principalTable: "Elections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Brackets_WaifuContenders_FirstWaifuId",
                        column: x => x.FirstWaifuId,
                        principalTable: "WaifuContenders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Brackets_WaifuContenders_SecondWaifuId",
                        column: x => x.SecondWaifuId,
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
                    UserId = table.Column<ulong>(type: "INTEGER", nullable: true),
                    VotedWaifuId = table.Column<long>(type: "INTEGER", nullable: true),
                    WaifuContenderId = table.Column<long>(type: "INTEGER", nullable: true)
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
                        name: "FK_Votes_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Votes_Waifus_VotedWaifuId",
                        column: x => x.VotedWaifuId,
                        principalTable: "Waifus",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Votes_WaifuContenders_WaifuContenderId",
                        column: x => x.WaifuContenderId,
                        principalTable: "WaifuContenders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Brackets_ElectionId",
                table: "Brackets",
                column: "ElectionId");

            migrationBuilder.CreateIndex(
                name: "IX_Brackets_FirstWaifuId",
                table: "Brackets",
                column: "FirstWaifuId");

            migrationBuilder.CreateIndex(
                name: "IX_Brackets_SecondWaifuId",
                table: "Brackets",
                column: "SecondWaifuId");

            migrationBuilder.CreateIndex(
                name: "IX_Elections_AuthorId",
                table: "Elections",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_Votes_BracketId",
                table: "Votes",
                column: "BracketId");

            migrationBuilder.CreateIndex(
                name: "IX_Votes_UserId",
                table: "Votes",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Votes_VotedWaifuId",
                table: "Votes",
                column: "VotedWaifuId");

            migrationBuilder.CreateIndex(
                name: "IX_Votes_WaifuContenderId",
                table: "Votes",
                column: "WaifuContenderId");

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

            migrationBuilder.CreateIndex(
                name: "IX_Waifus_UserId",
                table: "Waifus",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Votes");

            migrationBuilder.DropTable(
                name: "Brackets");

            migrationBuilder.DropTable(
                name: "WaifuContenders");

            migrationBuilder.DropTable(
                name: "Elections");

            migrationBuilder.DropTable(
                name: "Waifus");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
