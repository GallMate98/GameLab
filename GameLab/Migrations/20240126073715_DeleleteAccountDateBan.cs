using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameLab.Migrations
{
    /// <inheritdoc />
    public partial class DeleleteAccountDateBan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccountDateBan",
                table: "AspNetUsers");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "AccountDateBan",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true);
        }
    }
}
