using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace GameLab.Migrations
{
    /// <inheritdoc />
    public partial class GameTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Games",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Url = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Games", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Games",
                columns: new[] { "Id", "Description", "ImageUrl", "Name", "Url" },
                values: new object[,]
                {
                    { new Guid("0230088f-fe73-457d-851c-054053080059"), "This is a tic-tac-toe game.", "https://static-00.iconduck.com/assets.00/tic-tac-toe-icon-2048x2048-g58f0u84.png", "Tic-tac-toe", "tic-tac-toe" },
                    { new Guid("161bed46-0ef3-44ab-8646-9cd88a475c2b"), "This is a Nine Men's Morris game.", "https://play-lh.googleusercontent.com/y91Y53dmNXPmdy_k5KNAPzyVERChcwwH6A_ZHmBXsfrMYQfk_nlN2HLLmH1OlaLs0Q", "Nine Men's Morris", "nine-mens-morris" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Games");
        }
    }
}
