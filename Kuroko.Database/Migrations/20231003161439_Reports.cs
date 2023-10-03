using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Kuroko.Database.Migrations
{
    /// <inheritdoc />
    public partial class Reports : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GuildReports",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GuildId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    ReportCategoryId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    TranscriptsChannelId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    RecordMessages = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GuildReports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GuildReports_Guilds_GuildId",
                        column: x => x.GuildId,
                        principalTable: "Guilds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReportHandler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: true),
                    RoleId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    Level = table.Column<int>(type: "integer", nullable: false),
                    ReportsEntityId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportHandler", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReportHandler_GuildReports_ReportsEntityId",
                        column: x => x.ReportsEntityId,
                        principalTable: "GuildReports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GuildReports_GuildId",
                table: "GuildReports",
                column: "GuildId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReportHandler_ReportsEntityId",
                table: "ReportHandler",
                column: "ReportsEntityId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReportHandler");

            migrationBuilder.DropTable(
                name: "GuildReports");
        }
    }
}
