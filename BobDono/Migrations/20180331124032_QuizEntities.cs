using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace BobDono.Migrations
{
    public partial class QuizEntities : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "QuizQuestion",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    Answers = table.Column<string[]>(nullable: true),
                    Author = table.Column<string>(nullable: true),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    Hint = table.Column<string>(nullable: true),
                    Points = table.Column<int>(nullable: false),
                    Question = table.Column<string>(nullable: true),
                    QuestionBatch = table.Column<int>(nullable: false),
                    ReactionFailure = table.Column<string>(nullable: true),
                    ReactionSuccess = table.Column<string>(nullable: true),
                    Set = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuizQuestion", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "QuizSession",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    AdditonalScoreFromPreviousBatches = table.Column<int>(nullable: false),
                    CompletedBatch = table.Column<int>(nullable: true),
                    Finished = table.Column<DateTime>(nullable: false),
                    QuestionsCount = table.Column<int>(nullable: false),
                    RemainingChances = table.Column<int>(nullable: false),
                    Score = table.Column<int>(nullable: false),
                    SessionBatch = table.Column<int>(nullable: false),
                    Set = table.Column<int>(nullable: false),
                    Started = table.Column<DateTime>(nullable: false),
                    Status = table.Column<int>(nullable: false),
                    TotalChances = table.Column<int>(nullable: false),
                    UserId = table.Column<long>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuizSession", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuizSession_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "QuizAnswer",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    Answer = table.Column<string>(nullable: true),
                    IsCorrect = table.Column<bool>(nullable: false),
                    QuestionId = table.Column<long>(nullable: true),
                    SessionId = table.Column<long>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuizAnswer", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuizAnswer_QuizQuestion_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "QuizQuestion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_QuizAnswer_QuizSession_SessionId",
                        column: x => x.SessionId,
                        principalTable: "QuizSession",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_QuizAnswer_QuestionId",
                table: "QuizAnswer",
                column: "QuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_QuizAnswer_SessionId",
                table: "QuizAnswer",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_QuizSession_UserId",
                table: "QuizSession",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "QuizAnswer");

            migrationBuilder.DropTable(
                name: "QuizQuestion");

            migrationBuilder.DropTable(
                name: "QuizSession");
        }
    }
}
