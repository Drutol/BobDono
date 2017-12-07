using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace BobDono.Migrations
{
    public partial class UpdateHallOfFameMember : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CommandName",
                table: "HallOfFameMembers",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "ContenderMessageId",
                table: "HallOfFameMembers",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<string>(
                name: "ElectionName",
                table: "HallOfFameMembers",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "HallOfFameMembers",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "InfoMessageId",
                table: "HallOfFameMembers",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "OwnerId",
                table: "HallOfFameMembers",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "SeparatorMessageId",
                table: "HallOfFameMembers",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateIndex(
                name: "IX_HallOfFameMembers_OwnerId",
                table: "HallOfFameMembers",
                column: "OwnerId");

            migrationBuilder.AddForeignKey(
                name: "FK_HallOfFameMembers_Users_OwnerId",
                table: "HallOfFameMembers",
                column: "OwnerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HallOfFameMembers_Users_OwnerId",
                table: "HallOfFameMembers");

            migrationBuilder.DropIndex(
                name: "IX_HallOfFameMembers_OwnerId",
                table: "HallOfFameMembers");

            migrationBuilder.DropColumn(
                name: "CommandName",
                table: "HallOfFameMembers");

            migrationBuilder.DropColumn(
                name: "ContenderMessageId",
                table: "HallOfFameMembers");

            migrationBuilder.DropColumn(
                name: "ElectionName",
                table: "HallOfFameMembers");

            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "HallOfFameMembers");

            migrationBuilder.DropColumn(
                name: "InfoMessageId",
                table: "HallOfFameMembers");

            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "HallOfFameMembers");

            migrationBuilder.DropColumn(
                name: "SeparatorMessageId",
                table: "HallOfFameMembers");
        }
    }
}
