using System;
using Bartender.Data.Enums;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Bartender.Data.Migrations
{
    /// <inheritdoc />
    public partial class CreatePlaceImageTable : Migration
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
                .OldAnnotation("Npgsql:Enum:employeerole", "admin,manager,owner,regular")
                .OldAnnotation("Npgsql:Enum:orderstatus", "approved,cancelled,closed,created,delivered,paid,payment_requested")
                .OldAnnotation("Npgsql:Enum:paymenttype", "cash,creditcard,other")
                .OldAnnotation("Npgsql:Enum:subscriptiontier", "basic,enterprise,none,premium,standard,trial")
                .OldAnnotation("Npgsql:Enum:tablestatus", "empty,occupied,reserved");

            migrationBuilder.AddColumn<string>(
                name: "description",
                table: "places",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "place_pictures",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    place_id = table.Column<int>(type: "integer", nullable: false),
                    url = table.Column<string>(type: "text", nullable: false),
                    image_type = table.Column<ImageType>(type: "picturetype", nullable: false),
                    is_visible = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_place_pictures", x => x.id);
                    table.ForeignKey(
                        name: "FK_place_pictures_places_place_id",
                        column: x => x.place_id,
                        principalTable: "places",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_place_pictures_place_id",
                table: "place_pictures",
                column: "place_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "place_pictures");

            migrationBuilder.DropColumn(
                name: "description",
                table: "places");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:employeerole", "admin,manager,owner,regular")
                .Annotation("Npgsql:Enum:orderstatus", "approved,cancelled,closed,created,delivered,paid,payment_requested")
                .Annotation("Npgsql:Enum:paymenttype", "cash,creditcard,other")
                .Annotation("Npgsql:Enum:subscriptiontier", "basic,enterprise,none,premium,standard,trial")
                .Annotation("Npgsql:Enum:tablestatus", "empty,occupied,reserved")
                .OldAnnotation("Npgsql:Enum:employeerole", "admin,manager,owner,regular")
                .OldAnnotation("Npgsql:Enum:orderstatus", "approved,cancelled,closed,created,delivered,paid,payment_requested")
                .OldAnnotation("Npgsql:Enum:paymenttype", "cash,creditcard,other")
                .OldAnnotation("Npgsql:Enum:picturetype", "banner,blueprints,events,gallery,logo,promotion")
                .OldAnnotation("Npgsql:Enum:subscriptiontier", "basic,enterprise,none,premium,standard,trial")
                .OldAnnotation("Npgsql:Enum:tablestatus", "empty,occupied,reserved");
        }
    }
}
