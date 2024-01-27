using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameLab.Migrations
{
    /// <inheritdoc />
    public partial class AddAccountDateBan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "AccountDateBan",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: false,
                defaultValue: DateTime.MinValue);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccountDateBan",
                table: "AspNetUsers");
        }
    }
}
