using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kuroko.Database.Migrations
{
    /// <inheritdoc />
    public partial class BanSyncUpdate2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Mode",
                table: "BanSyncProperties",
                newName: "HostMode");

            migrationBuilder.AddColumn<int>(
                name: "ClientMode",
                table: "BanSyncProperties",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ClientMode",
                table: "BanSyncProperties");

            migrationBuilder.RenameColumn(
                name: "HostMode",
                table: "BanSyncProperties",
                newName: "Mode");
        }
    }
}
