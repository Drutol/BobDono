using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace BobDono.Migrations
{
    public partial class AddApprovalsToElectionTheme : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Approvals",
                table: "ElectionThemes");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreateDate",
                table: "ElectionThemes",
                type: "timestamp",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "ElectionCreateDate",
                table: "ElectionThemes",
                type: "timestamp",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

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
                name: "IX_UserTheme_ThemeId",
                table: "UserTheme",
                column: "ThemeId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserTheme");

            migrationBuilder.DropColumn(
                name: "CreateDate",
                table: "ElectionThemes");

            migrationBuilder.DropColumn(
                name: "ElectionCreateDate",
                table: "ElectionThemes");

            migrationBuilder.AddColumn<int>(
                name: "Approvals",
                table: "ElectionThemes",
                nullable: false,
                defaultValue: 0);
        }
    }
}
