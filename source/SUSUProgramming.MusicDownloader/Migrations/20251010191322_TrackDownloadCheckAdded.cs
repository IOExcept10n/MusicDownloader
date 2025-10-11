using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SUSUProgramming.MusicDownloader.Migrations
{
    /// <inheritdoc />
    public partial class TrackDownloadCheckAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TrackPath",
                table: "TrackStats",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TrackPath",
                table: "TrackStats");
        }
    }
}
