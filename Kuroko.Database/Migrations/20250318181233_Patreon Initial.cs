using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Kuroko.Database.Migrations
{
    /// <inheritdoc />
    public partial class PatreonInitial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BanSyncProperties_Guilds_GuildId",
                table: "BanSyncProperties");

            migrationBuilder.RenameColumn(
                name: "GuildId",
                table: "BanSyncProperties",
                newName: "RootId");

            migrationBuilder.RenameIndex(
                name: "IX_BanSyncProperties_GuildId",
                table: "BanSyncProperties",
                newName: "IX_BanSyncProperties_RootId");

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<decimal>(type: "numeric(20,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PatreonProperties",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RootId = table.Column<decimal>(type: "numeric(20,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PatreonProperties", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PatreonProperties_Users_RootId",
                        column: x => x.RootId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PremiumKeys",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PatreonPropertiesId = table.Column<int>(type: "integer", nullable: false),
                    GuildId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    Key = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ExpiresAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PremiumKeys", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PremiumKeys_Guilds_GuildId",
                        column: x => x.GuildId,
                        principalTable: "Guilds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PremiumKeys_PatreonProperties_PatreonPropertiesId",
                        column: x => x.PatreonPropertiesId,
                        principalTable: "PatreonProperties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PatreonProperties_RootId",
                table: "PatreonProperties",
                column: "RootId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PremiumKeys_GuildId",
                table: "PremiumKeys",
                column: "GuildId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PremiumKeys_PatreonPropertiesId",
                table: "PremiumKeys",
                column: "PatreonPropertiesId");

            migrationBuilder.AddForeignKey(
                name: "FK_BanSyncProperties_Guilds_RootId",
                table: "BanSyncProperties",
                column: "RootId",
                principalTable: "Guilds",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BanSyncProperties_Guilds_RootId",
                table: "BanSyncProperties");

            migrationBuilder.DropTable(
                name: "PremiumKeys");

            migrationBuilder.DropTable(
                name: "PatreonProperties");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.RenameColumn(
                name: "RootId",
                table: "BanSyncProperties",
                newName: "GuildId");

            migrationBuilder.RenameIndex(
                name: "IX_BanSyncProperties_RootId",
                table: "BanSyncProperties",
                newName: "IX_BanSyncProperties_GuildId");

            migrationBuilder.AddForeignKey(
                name: "FK_BanSyncProperties_Guilds_GuildId",
                table: "BanSyncProperties",
                column: "GuildId",
                principalTable: "Guilds",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
