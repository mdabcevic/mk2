using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bartender.Data.Migrations
{
    /// <inheritdoc />
    public partial class PlaceAndCityCoordinates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "latitude",
                table: "places",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "longitude",
                table: "places",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "latitude",
                table: "city",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "longitude",
                table: "city",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "latitude",
                table: "places");

            migrationBuilder.DropColumn(
                name: "longitude",
                table: "places");

            migrationBuilder.DropColumn(
                name: "latitude",
                table: "city");

            migrationBuilder.DropColumn(
                name: "longitude",
                table: "city");
        }
    }
}
