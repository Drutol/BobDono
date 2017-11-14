using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace BobDono.Migrations
{
    public partial class AddOgraphyToWaifusAndIdEmbedToContender : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string[]>(
                name: "Animeography",
                table: "Waifus",
                type: "text[]",
                nullable: true);

            migrationBuilder.AddColumn<string[]>(
                name: "Mangaography",
                table: "Waifus",
                type: "text[]",
                nullable: true);

            migrationBuilder.AddColumn<string[]>(
                name: "Voiceactors",
                table: "Waifus",
                type: "text[]",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "SubmissionEmbedId",
                table: "WaifuContenders",
                type: "int8",
                nullable: false,
                defaultValue: 0L);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Animeography",
                table: "Waifus");

            migrationBuilder.DropColumn(
                name: "Mangaography",
                table: "Waifus");

            migrationBuilder.DropColumn(
                name: "Voiceactors",
                table: "Waifus");

            migrationBuilder.DropColumn(
                name: "SubmissionEmbedId",
                table: "WaifuContenders");
        }
    }
}
