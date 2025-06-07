using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bartender.Data.Migrations
{
    /// <inheritdoc />
    public partial class googleMapIframeLink : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "google_map_iframe_link",
                table: "places",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "google_map_iframe_link",
                table: "places");
        }
    }
}
