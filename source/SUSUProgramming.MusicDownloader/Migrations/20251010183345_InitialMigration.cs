using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SUSUProgramming.MusicDownloader.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TrackStats",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ArtistString = table.Column<string>(type: "TEXT", nullable: false),
                    TitleString = table.Column<string>(type: "TEXT", nullable: false),
                    ListenCount = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrackStats", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TrackStats");
        }
    }
}
