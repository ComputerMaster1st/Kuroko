using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kuroko.Database.Migrations
{
    /// <inheritdoc />
    public partial class ModLogV3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AuditLog",
                table: "GuildModLogs",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Ban",
                table: "GuildModLogs",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Kick",
                table: "GuildModLogs",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ServerMute",
                table: "GuildModLogs",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Timeout",
                table: "GuildModLogs",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AuditLog",
                table: "GuildModLogs");

            migrationBuilder.DropColumn(
                name: "Ban",
                table: "GuildModLogs");

            migrationBuilder.DropColumn(
                name: "Kick",
                table: "GuildModLogs");

            migrationBuilder.DropColumn(
                name: "ServerMute",
                table: "GuildModLogs");

            migrationBuilder.DropColumn(
                name: "Timeout",
                table: "GuildModLogs");
        }
    }
}
