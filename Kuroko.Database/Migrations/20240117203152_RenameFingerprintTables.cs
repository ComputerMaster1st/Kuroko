using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kuroko.Database.Migrations
{
    /// <inheritdoc />
    public partial class RenameFingerprintTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Hashs_SubFingerprints_SubFingerprintId",
                table: "Hashs");

            migrationBuilder.DropForeignKey(
                name: "FK_SubFingerprints_TrackData_TrackDataId",
                table: "SubFingerprints");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TrackData",
                table: "TrackData");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SubFingerprints",
                table: "SubFingerprints");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Hashs",
                table: "Hashs");

            migrationBuilder.RenameTable(
                name: "TrackData",
                newName: "SFPTrackData");

            migrationBuilder.RenameTable(
                name: "SubFingerprints",
                newName: "SFPSubFingerprints");

            migrationBuilder.RenameTable(
                name: "Hashs",
                newName: "SFPHashes");

            migrationBuilder.RenameIndex(
                name: "IX_SubFingerprints_TrackDataId",
                table: "SFPSubFingerprints",
                newName: "IX_SFPSubFingerprints_TrackDataId");

            migrationBuilder.RenameIndex(
                name: "IX_Hashs_SubFingerprintId",
                table: "SFPHashes",
                newName: "IX_SFPHashes_SubFingerprintId");

            migrationBuilder.RenameIndex(
                name: "IX_Hashs_IndexedHash",
                table: "SFPHashes",
                newName: "IX_SFPHashes_IndexedHash");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SFPTrackData",
                table: "SFPTrackData",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SFPSubFingerprints",
                table: "SFPSubFingerprints",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SFPHashes",
                table: "SFPHashes",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SFPHashes_SFPSubFingerprints_SubFingerprintId",
                table: "SFPHashes",
                column: "SubFingerprintId",
                principalTable: "SFPSubFingerprints",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SFPSubFingerprints_SFPTrackData_TrackDataId",
                table: "SFPSubFingerprints",
                column: "TrackDataId",
                principalTable: "SFPTrackData",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SFPHashes_SFPSubFingerprints_SubFingerprintId",
                table: "SFPHashes");

            migrationBuilder.DropForeignKey(
                name: "FK_SFPSubFingerprints_SFPTrackData_TrackDataId",
                table: "SFPSubFingerprints");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SFPTrackData",
                table: "SFPTrackData");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SFPSubFingerprints",
                table: "SFPSubFingerprints");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SFPHashes",
                table: "SFPHashes");

            migrationBuilder.RenameTable(
                name: "SFPTrackData",
                newName: "TrackData");

            migrationBuilder.RenameTable(
                name: "SFPSubFingerprints",
                newName: "SubFingerprints");

            migrationBuilder.RenameTable(
                name: "SFPHashes",
                newName: "Hashs");

            migrationBuilder.RenameIndex(
                name: "IX_SFPSubFingerprints_TrackDataId",
                table: "SubFingerprints",
                newName: "IX_SubFingerprints_TrackDataId");

            migrationBuilder.RenameIndex(
                name: "IX_SFPHashes_SubFingerprintId",
                table: "Hashs",
                newName: "IX_Hashs_SubFingerprintId");

            migrationBuilder.RenameIndex(
                name: "IX_SFPHashes_IndexedHash",
                table: "Hashs",
                newName: "IX_Hashs_IndexedHash");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TrackData",
                table: "TrackData",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SubFingerprints",
                table: "SubFingerprints",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Hashs",
                table: "Hashs",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Hashs_SubFingerprints_SubFingerprintId",
                table: "Hashs",
                column: "SubFingerprintId",
                principalTable: "SubFingerprints",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SubFingerprints_TrackData_TrackDataId",
                table: "SubFingerprints",
                column: "TrackDataId",
                principalTable: "TrackData",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
