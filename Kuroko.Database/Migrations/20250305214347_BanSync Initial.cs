using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Kuroko.Database.Migrations
{
    /// <inheritdoc />
    public partial class BanSyncInitial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Guilds",
                columns: table => new
                {
                    Id = table.Column<decimal>(type: "numeric(20,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Guilds", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BanSyncProperties",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GuildId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    SyncId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    AllowRequests = table.Column<bool>(type: "boolean", nullable: false),
                    Mode = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BanSyncProperties", x => x.Id);
                    table.UniqueConstraint("AK_BanSyncProperties_SyncId", x => x.SyncId);
                    table.ForeignKey(
                        name: "FK_BanSyncProperties_Guilds_GuildId",
                        column: x => x.GuildId,
                        principalTable: "Guilds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BanSyncProfiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    HostSyncId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClientSyncId = table.Column<Guid>(type: "uuid", nullable: false),
                    Mode = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BanSyncProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BanSyncProfiles_BanSyncProperties_ClientSyncId",
                        column: x => x.ClientSyncId,
                        principalTable: "BanSyncProperties",
                        principalColumn: "SyncId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BanSyncProfiles_BanSyncProperties_HostSyncId",
                        column: x => x.HostSyncId,
                        principalTable: "BanSyncProperties",
                        principalColumn: "SyncId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BanSyncProfiles_ClientSyncId",
                table: "BanSyncProfiles",
                column: "ClientSyncId");

            migrationBuilder.CreateIndex(
                name: "IX_BanSyncProfiles_HostSyncId",
                table: "BanSyncProfiles",
                column: "HostSyncId");

            migrationBuilder.CreateIndex(
                name: "IX_BanSyncProperties_GuildId",
                table: "BanSyncProperties",
                column: "GuildId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BanSyncProfiles");

            migrationBuilder.DropTable(
                name: "BanSyncProperties");

            migrationBuilder.DropTable(
                name: "Guilds");
        }
    }
}
