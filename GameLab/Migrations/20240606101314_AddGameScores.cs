using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace GameLab.Migrations
{
    /// <inheritdoc />
    public partial class AddGameScores : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Games",
                keyColumn: "Id",
                keyValue: new Guid("ca9d7501-2696-4b8c-926f-2d91dbb1c21c"));

            migrationBuilder.DeleteData(
                table: "Games",
                keyColumn: "Id",
                keyValue: new Guid("e6a74c17-c054-4668-b478-6ecd6d8fc37b"));

            migrationBuilder.CreateTable(
                name: "GameScores",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GameId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Score = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameScores", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GameScores_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GameScores_Games_GameId",
                        column: x => x.GameId,
                        principalTable: "Games",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Games",
                columns: new[] { "Id", "Description", "ImageUrl", "Name", "Url" },
                values: new object[,]
                {
                    { new Guid("19ff08a4-22b6-4f19-8803-7941e1edc40b"), "This is a Nine Men's Morris game.", "https://play-lh.googleusercontent.com/y91Y53dmNXPmdy_k5KNAPzyVERChcwwH6A_ZHmBXsfrMYQfk_nlN2HLLmH1OlaLs0Q", "Nine Men's Morris", "nine-mens-morris" },
                    { new Guid("3bdccbe1-066f-40d4-a719-2726b39f794d"), "This is a tic-tac-toe game.", "https://static-00.iconduck.com/assets.00/tic-tac-toe-icon-2048x2048-g58f0u84.png", "Tic-tac-toe", "tic-tac-toe" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_GameScores_GameId",
                table: "GameScores",
                column: "GameId");

            migrationBuilder.CreateIndex(
                name: "IX_GameScores_UserId",
                table: "GameScores",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GameScores");

            migrationBuilder.DeleteData(
                table: "Games",
                keyColumn: "Id",
                keyValue: new Guid("19ff08a4-22b6-4f19-8803-7941e1edc40b"));

            migrationBuilder.DeleteData(
                table: "Games",
                keyColumn: "Id",
                keyValue: new Guid("3bdccbe1-066f-40d4-a719-2726b39f794d"));

            migrationBuilder.InsertData(
                table: "Games",
                columns: new[] { "Id", "Description", "ImageUrl", "Name", "Url" },
                values: new object[,]
                {
                    { new Guid("ca9d7501-2696-4b8c-926f-2d91dbb1c21c"), "This is a tic-tac-toe game.", "https://static-00.iconduck.com/assets.00/tic-tac-toe-icon-2048x2048-g58f0u84.png", "Tic-tac-toe", "tic-tac-toe" },
                    { new Guid("e6a74c17-c054-4668-b478-6ecd6d8fc37b"), "This is a Nine Men's Morris game.", "https://play-lh.googleusercontent.com/y91Y53dmNXPmdy_k5KNAPzyVERChcwwH6A_ZHmBXsfrMYQfk_nlN2HLLmH1OlaLs0Q", "Nine Men's Morris", "nine-mens-morris" }
                });
        }
    }
}
