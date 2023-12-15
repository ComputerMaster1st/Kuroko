using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Kuroko.Database.Migrations
{
    /// <inheritdoc />
    public partial class blackbox_v2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EnableBlackbox",
                table: "GuildModLogs");

            migrationBuilder.DropColumn(
                name: "SaveAttachments",
                table: "GuildModLogs");

            migrationBuilder.CreateTable(
                name: "Blackboxes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GuildId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    SaveAttachments = table.Column<bool>(type: "boolean", nullable: false),
                    SyncModLog = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Blackboxes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Blackboxes_Guilds_GuildId",
                        column: x => x.GuildId,
                        principalTable: "Guilds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Blackboxes_GuildId",
                table: "Blackboxes",
                column: "GuildId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Blackboxes");

            migrationBuilder.AddColumn<bool>(
                name: "EnableBlackbox",
                table: "GuildModLogs",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "SaveAttachments",
                table: "GuildModLogs",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
