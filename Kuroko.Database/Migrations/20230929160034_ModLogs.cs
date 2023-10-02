using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Kuroko.Database.Migrations
{
    /// <inheritdoc />
    public partial class ModLogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ModLogEntityId",
                table: "UlongEntity",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "GuildModLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GuildId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    LogChannelId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    DeletedMessages = table.Column<bool>(type: "boolean", nullable: false),
                    EditedMessages = table.Column<bool>(type: "boolean", nullable: false),
                    Join = table.Column<bool>(type: "boolean", nullable: false),
                    Leave = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GuildModLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GuildModLogs_Guilds_GuildId",
                        column: x => x.GuildId,
                        principalTable: "Guilds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UlongEntity_ModLogEntityId",
                table: "UlongEntity",
                column: "ModLogEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_GuildModLogs_GuildId",
                table: "GuildModLogs",
                column: "GuildId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_UlongEntity_GuildModLogs_ModLogEntityId",
                table: "UlongEntity",
                column: "ModLogEntityId",
                principalTable: "GuildModLogs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UlongEntity_GuildModLogs_ModLogEntityId",
                table: "UlongEntity");

            migrationBuilder.DropTable(
                name: "GuildModLogs");

            migrationBuilder.DropIndex(
                name: "IX_UlongEntity_ModLogEntityId",
                table: "UlongEntity");

            migrationBuilder.DropColumn(
                name: "ModLogEntityId",
                table: "UlongEntity");
        }
    }
}
