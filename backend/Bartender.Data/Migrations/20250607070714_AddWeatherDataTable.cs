using System;
using Bartender.Data.Enums;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Bartender.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddWeatherDataTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:employeerole", "admin,manager,owner,regular")
                .Annotation("Npgsql:Enum:orderstatus", "approved,cancelled,closed,created,delivered,paid,payment_requested")
                .Annotation("Npgsql:Enum:paymenttype", "cash,creditcard,other")
                .Annotation("Npgsql:Enum:picturetype", "banner,blueprints,events,gallery,logo,promotion")
                .Annotation("Npgsql:Enum:subscriptiontier", "basic,enterprise,none,premium,standard,trial")
                .Annotation("Npgsql:Enum:tablestatus", "empty,occupied,reserved")
                .Annotation("Npgsql:Enum:weathertype", "clear,cloudy,rainy,severe_weather,snowy,unknown")
                .OldAnnotation("Npgsql:Enum:employeerole", "admin,manager,owner,regular")
                .OldAnnotation("Npgsql:Enum:orderstatus", "approved,cancelled,closed,created,delivered,paid,payment_requested")
                .OldAnnotation("Npgsql:Enum:paymenttype", "cash,creditcard,other")
                .OldAnnotation("Npgsql:Enum:picturetype", "banner,blueprints,events,gallery,logo,promotion")
                .OldAnnotation("Npgsql:Enum:subscriptiontier", "basic,enterprise,none,premium,standard,trial")
                .OldAnnotation("Npgsql:Enum:tablestatus", "empty,occupied,reserved");

            migrationBuilder.CreateTable(
                name: "weather_datas",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    date_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    city_id = table.Column<int>(type: "integer", nullable: false),
                    temperature = table.Column<double>(type: "double precision", nullable: true),
                    weather_type = table.Column<WeatherType>(type: "weathertype", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_weather_datas", x => x.id);
                    table.ForeignKey(
                        name: "FK_weather_datas_city_city_id",
                        column: x => x.city_id,
                        principalTable: "city",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_weather_datas_city_id",
                table: "weather_datas",
                column: "city_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "weather_datas");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:employeerole", "admin,manager,owner,regular")
                .Annotation("Npgsql:Enum:orderstatus", "approved,cancelled,closed,created,delivered,paid,payment_requested")
                .Annotation("Npgsql:Enum:paymenttype", "cash,creditcard,other")
                .Annotation("Npgsql:Enum:picturetype", "banner,blueprints,events,gallery,logo,promotion")
                .Annotation("Npgsql:Enum:subscriptiontier", "basic,enterprise,none,premium,standard,trial")
                .Annotation("Npgsql:Enum:tablestatus", "empty,occupied,reserved")
                .OldAnnotation("Npgsql:Enum:employeerole", "admin,manager,owner,regular")
                .OldAnnotation("Npgsql:Enum:orderstatus", "approved,cancelled,closed,created,delivered,paid,payment_requested")
                .OldAnnotation("Npgsql:Enum:paymenttype", "cash,creditcard,other")
                .OldAnnotation("Npgsql:Enum:picturetype", "banner,blueprints,events,gallery,logo,promotion")
                .OldAnnotation("Npgsql:Enum:subscriptiontier", "basic,enterprise,none,premium,standard,trial")
                .OldAnnotation("Npgsql:Enum:tablestatus", "empty,occupied,reserved")
                .OldAnnotation("Npgsql:Enum:weathertype", "clear,cloudy,rainy,severe_weather,snowy,unknown");
        }
    }
}
