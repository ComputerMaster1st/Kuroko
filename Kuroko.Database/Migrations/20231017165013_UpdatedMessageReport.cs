using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kuroko.Database.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedMessageReport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReportedContent",
                table: "Tickets");

            migrationBuilder.AddColumn<decimal>(
                name: "ReportedMessageId",
                table: "Tickets",
                type: "numeric(20,0)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReportedMessageId",
                table: "Tickets");

            migrationBuilder.AddColumn<string>(
                name: "ReportedContent",
                table: "Tickets",
                type: "text",
                nullable: true);
        }
    }
}
