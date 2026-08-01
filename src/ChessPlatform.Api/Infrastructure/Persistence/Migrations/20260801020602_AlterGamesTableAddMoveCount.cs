using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChessPlatform.Api.Migrations
{
    /// <inheritdoc />
    public partial class AlterGamesTableAddMoveCount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MoveCount",
                table: "Games",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MoveCount",
                table: "Games");
        }
    }
}
