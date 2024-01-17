using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Kuroko.Database.Migrations
{
    /// <inheritdoc />
    public partial class SeparateMetaTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Artist",
                table: "SFPTrackData");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "SFPTrackData");

            migrationBuilder.RenameColumn(
                name: "TrackInfoId",
                table: "SFPTrackData",
                newName: "SongId");

            migrationBuilder.CreateTable(
                name: "SongInfo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Title = table.Column<string>(type: "text", nullable: true),
                    Artist = table.Column<string>(type: "text", nullable: true),
                    Album = table.Column<string>(type: "text", nullable: true),
                    Length = table.Column<TimeSpan>(type: "interval", nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SongInfo", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SongInfo");

            migrationBuilder.RenameColumn(
                name: "SongId",
                table: "SFPTrackData",
                newName: "TrackInfoId");

            migrationBuilder.AddColumn<string>(
                name: "Artist",
                table: "SFPTrackData",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "SFPTrackData",
                type: "text",
                nullable: true);
        }
    }
}
