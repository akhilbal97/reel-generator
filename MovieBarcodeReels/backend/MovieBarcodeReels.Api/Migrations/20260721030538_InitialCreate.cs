using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MovieBarcodeReels.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Movies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Title = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    Year = table.Column<int>(type: "INTEGER", nullable: false),
                    Trivia = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false),
                    Genre = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    Difficulty = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    BarcodeImagePath = table.Column<string>(type: "TEXT", maxLength: 512, nullable: true),
                    MusicPath = table.Column<string>(type: "TEXT", maxLength: 512, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Movies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GeneratedVideos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MovieId = table.Column<int>(type: "INTEGER", nullable: false),
                    FilePath = table.Column<string>(type: "TEXT", maxLength: 512, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DurationMs = table.Column<long>(type: "INTEGER", nullable: false),
                    UsedPlaceholder = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GeneratedVideos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GeneratedVideos_Movies_MovieId",
                        column: x => x.MovieId,
                        principalTable: "Movies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GeneratedVideos_MovieId",
                table: "GeneratedVideos",
                column: "MovieId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GeneratedVideos");

            migrationBuilder.DropTable(
                name: "Movies");
        }
    }
}
