using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace BobDono.Migrations
{
    public partial class NotesToMatchupPairs : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FirstNotes",
                table: "MatchupPair",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SecondNotes",
                table: "MatchupPair",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FirstNotes",
                table: "MatchupPair");

            migrationBuilder.DropColumn(
                name: "SecondNotes",
                table: "MatchupPair");
        }
    }
}
