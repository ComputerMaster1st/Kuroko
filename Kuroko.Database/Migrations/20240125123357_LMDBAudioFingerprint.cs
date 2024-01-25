using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Kuroko.Database.Migrations
{
    /// <inheritdoc />
    public partial class LMDBAudioFingerprint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SFPHashes");

            migrationBuilder.DropTable(
                name: "SFPSubFingerprints");

            migrationBuilder.DropTable(
                name: "SFPTrackData");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SFPTrackData",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Length = table.Column<double>(type: "double precision", nullable: false),
                    SongId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SFPTrackData", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SFPSubFingerprints",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TrackDataId = table.Column<int>(type: "integer", nullable: false),
                    OriginalPoint = table.Column<byte[]>(type: "bytea", nullable: true),
                    SequenceAt = table.Column<float>(type: "real", nullable: false),
                    SequenceNumber = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SFPSubFingerprints", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SFPSubFingerprints_SFPTrackData_TrackDataId",
                        column: x => x.TrackDataId,
                        principalTable: "SFPTrackData",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SFPHashes",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SubFingerprintId = table.Column<long>(type: "bigint", nullable: false),
                    IndexedHash = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SFPHashes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SFPHashes_SFPSubFingerprints_SubFingerprintId",
                        column: x => x.SubFingerprintId,
                        principalTable: "SFPSubFingerprints",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SFPHashes_IndexedHash",
                table: "SFPHashes",
                column: "IndexedHash");

            migrationBuilder.CreateIndex(
                name: "IX_SFPHashes_SubFingerprintId",
                table: "SFPHashes",
                column: "SubFingerprintId");

            migrationBuilder.CreateIndex(
                name: "IX_SFPSubFingerprints_TrackDataId",
                table: "SFPSubFingerprints",
                column: "TrackDataId");
        }
    }
}
