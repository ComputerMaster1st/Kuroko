using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Kuroko.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddedAudioFingerprint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TrackData",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TrackInfoId = table.Column<string>(type: "text", nullable: true),
                    Artist = table.Column<string>(type: "text", nullable: true),
                    Title = table.Column<string>(type: "text", nullable: true),
                    Length = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrackData", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SubFingerprints",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TrackDataId = table.Column<int>(type: "integer", nullable: false),
                    SequenceNumber = table.Column<long>(type: "bigint", nullable: false),
                    SequenceAt = table.Column<float>(type: "real", nullable: false),
                    OriginalPoint = table.Column<byte[]>(type: "bytea", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubFingerprints", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SubFingerprints_TrackData_TrackDataId",
                        column: x => x.TrackDataId,
                        principalTable: "TrackData",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Hashs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IndexedHash = table.Column<long>(type: "bigint", nullable: false),
                    SubFingerprintId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Hashs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Hashs_SubFingerprints_SubFingerprintId",
                        column: x => x.SubFingerprintId,
                        principalTable: "SubFingerprints",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Hashs_IndexedHash",
                table: "Hashs",
                column: "IndexedHash");

            migrationBuilder.CreateIndex(
                name: "IX_Hashs_SubFingerprintId",
                table: "Hashs",
                column: "SubFingerprintId");

            migrationBuilder.CreateIndex(
                name: "IX_SubFingerprints_TrackDataId",
                table: "SubFingerprints",
                column: "TrackDataId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Hashs");

            migrationBuilder.DropTable(
                name: "SubFingerprints");

            migrationBuilder.DropTable(
                name: "TrackData");
        }
    }
}
