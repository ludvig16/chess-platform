using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChessPlatform.Api.Migrations
{
    /// <inheritdoc />
    public partial class AlterGamesTableAddTurnStartedAndTurnExpiresAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "TurnExpiresAt",
                table: "Games",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "TurnStartedAt",
                table: "Games",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TurnExpiresAt",
                table: "Games");

            migrationBuilder.DropColumn(
                name: "TurnStartedAt",
                table: "Games");
        }
    }
}
