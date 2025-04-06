using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cantare.Migrations
{
    /// <inheritdoc />
    public partial class RemoveUnnecessaryGuessData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GuessModel_AvailableSongs_SongDatabaseKey",
                table: "GuessModel");

            migrationBuilder.DropIndex(
                name: "IX_GuessModel_SongDatabaseKey",
                table: "GuessModel");

            migrationBuilder.DropColumn(
                name: "GuessNumber",
                table: "GuessModel");

            migrationBuilder.DropColumn(
                name: "SongDatabaseKey",
                table: "GuessModel");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "GuessNumber",
                table: "GuessModel",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SongDatabaseKey",
                table: "GuessModel",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_GuessModel_SongDatabaseKey",
                table: "GuessModel",
                column: "SongDatabaseKey");

            migrationBuilder.AddForeignKey(
                name: "FK_GuessModel_AvailableSongs_SongDatabaseKey",
                table: "GuessModel",
                column: "SongDatabaseKey",
                principalTable: "AvailableSongs",
                principalColumn: "DatabaseKey",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
