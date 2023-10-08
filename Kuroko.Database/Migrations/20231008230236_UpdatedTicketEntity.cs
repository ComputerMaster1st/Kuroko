using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kuroko.Database.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedTicketEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "ChannelId",
                table: "Tickets",
                type: "numeric(20,0)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "SummaryMessageId",
                table: "Tickets",
                type: "numeric(20,0)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ChannelId",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "SummaryMessageId",
                table: "Tickets");
        }
    }
}
