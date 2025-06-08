using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bartender.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddWeatherForeignKeyToOrders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "weather_id",
                table: "orders",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_orders_weather_id",
                table: "orders",
                column: "weather_id");

            migrationBuilder.AddForeignKey(
                name: "FK_orders_weather_datas_weather_id",
                table: "orders",
                column: "weather_id",
                principalTable: "weather_datas",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_orders_weather_datas_weather_id",
                table: "orders");

            migrationBuilder.DropIndex(
                name: "IX_orders_weather_id",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "weather_id",
                table: "orders");
        }
    }
}
