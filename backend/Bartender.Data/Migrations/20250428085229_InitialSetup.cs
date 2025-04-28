using System;
using Bartender.Data.Enums;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Bartender.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialSetup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:employeerole", "admin,manager,owner,regular")
                .Annotation("Npgsql:Enum:orderstatus", "approved,cancelled,closed,created,delivered,paid,payment_requested")
                .Annotation("Npgsql:Enum:paymenttype", "cash,creditcard,other")
                .Annotation("Npgsql:Enum:subscriptiontier", "basic,enterprise,none,premium,standard,trial")
                .Annotation("Npgsql:Enum:tablestatus", "empty,occupied,reserved");

            migrationBuilder.CreateTable(
                name: "businesses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    oib = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    headquarters = table.Column<string>(type: "text", nullable: true),
                    subscriptiontier = table.Column<SubscriptionTier>(type: "subscriptiontier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_businesses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "cities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "customers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    username = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    password = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_customers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "productcategory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    parentcategory_id = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_productcategory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_productcategory_productcategory_parentcategory_id",
                        column: x => x.parentcategory_id,
                        principalTable: "productcategory",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "places",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    business_id = table.Column<int>(type: "integer", nullable: false),
                    address = table.Column<string>(type: "text", nullable: false),
                    city_id = table.Column<int>(type: "integer", nullable: false),
                    opensat = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    closesat = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_places", x => x.Id);
                    table.ForeignKey(
                        name: "FK_places_businesses_business_id",
                        column: x => x.business_id,
                        principalTable: "businesses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_places_cities_city_id",
                        column: x => x.city_id,
                        principalTable: "cities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "products",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    volume = table.Column<string>(type: "text", nullable: true),
                    category_id = table.Column<int>(type: "integer", nullable: false),
                    business_id = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_products", x => x.Id);
                    table.ForeignKey(
                        name: "FK_products_businesses_business_id",
                        column: x => x.business_id,
                        principalTable: "businesses",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_products_productcategory_category_id",
                        column: x => x.category_id,
                        principalTable: "productcategory",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "staff",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    place_id = table.Column<int>(type: "integer", nullable: false),
                    oib = table.Column<string>(type: "character varying(11)", maxLength: 11, nullable: false),
                    username = table.Column<string>(type: "text", nullable: false),
                    password = table.Column<string>(type: "text", nullable: false),
                    fullname = table.Column<string>(type: "text", nullable: false),
                    role = table.Column<EmployeeRole>(type: "employeerole", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_staff", x => x.Id);
                    table.ForeignKey(
                        name: "FK_staff_places_place_id",
                        column: x => x.place_id,
                        principalTable: "places",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tables",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    place_id = table.Column<int>(type: "integer", nullable: false),
                    label = table.Column<string>(type: "text", nullable: false),
                    seats = table.Column<int>(type: "integer", nullable: false),
                    width = table.Column<int>(type: "integer", nullable: false),
                    height = table.Column<int>(type: "integer", nullable: false),
                    xcoordinate = table.Column<decimal>(type: "numeric(6,2)", nullable: false),
                    ycoordinate = table.Column<decimal>(type: "numeric(6,2)", nullable: false),
                    status = table.Column<TableStatus>(type: "tablestatus", nullable: false),
                    qrsalt = table.Column<string>(type: "text", nullable: false),
                    isdisabled = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tables", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tables_places_place_id",
                        column: x => x.place_id,
                        principalTable: "places",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "menuitems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    place_id = table.Column<int>(type: "integer", nullable: false),
                    product_id = table.Column<int>(type: "integer", nullable: false),
                    price = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    isavailable = table.Column<bool>(type: "boolean", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_menuitems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_menuitems_places_place_id",
                        column: x => x.place_id,
                        principalTable: "places",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_menuitems_products_product_id",
                        column: x => x.product_id,
                        principalTable: "products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "reviews",
                columns: table => new
                {
                    product_id = table.Column<int>(type: "integer", nullable: false),
                    customer_id = table.Column<int>(type: "integer", nullable: false),
                    rating = table.Column<int>(type: "integer", nullable: false),
                    comment = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reviews", x => new { x.product_id, x.customer_id });
                    table.ForeignKey(
                        name: "FK_reviews_customers_customer_id",
                        column: x => x.customer_id,
                        principalTable: "customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_reviews_products_product_id",
                        column: x => x.product_id,
                        principalTable: "products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "guestsessiongroups",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    table_id = table.Column<int>(type: "integer", nullable: false),
                    passphrase = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_guestsessiongroups", x => x.id);
                    table.ForeignKey(
                        name: "FK_guestsessiongroups_tables_table_id",
                        column: x => x.table_id,
                        principalTable: "tables",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "guestsessions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    table_id = table.Column<int>(type: "integer", nullable: false),
                    group_id = table.Column<Guid>(type: "uuid", nullable: true),
                    token = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    isvalid = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_guestsessions", x => x.id);
                    table.ForeignKey(
                        name: "FK_guestsessions_guestsessiongroups_group_id",
                        column: x => x.group_id,
                        principalTable: "guestsessiongroups",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_guestsessions_tables_table_id",
                        column: x => x.table_id,
                        principalTable: "tables",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "orders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    table_id = table.Column<int>(type: "integer", nullable: false),
                    customer_id = table.Column<int>(type: "integer", nullable: true),
                    guest_session_id = table.Column<Guid>(type: "uuid", nullable: true),
                    status = table.Column<OrderStatus>(type: "orderstatus", nullable: false),
                    total_price = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    paymenttype = table.Column<PaymentType>(type: "paymenttype", nullable: false),
                    note = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_orders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_orders_customers_customer_id",
                        column: x => x.customer_id,
                        principalTable: "customers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_orders_guestsessions_guest_session_id",
                        column: x => x.guest_session_id,
                        principalTable: "guestsessions",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_orders_tables_table_id",
                        column: x => x.table_id,
                        principalTable: "tables",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "productsperorder",
                columns: table => new
                {
                    order_id = table.Column<int>(type: "integer", nullable: false),
                    menuitem_id = table.Column<int>(type: "integer", nullable: false),
                    item_price = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    discount = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    count = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_productsperorder", x => new { x.order_id, x.menuitem_id });
                    table.ForeignKey(
                        name: "FK_productsperorder_menuitems_menuitem_id",
                        column: x => x.menuitem_id,
                        principalTable: "menuitems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_productsperorder_orders_order_id",
                        column: x => x.order_id,
                        principalTable: "orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_guestsessiongroups_table_id",
                table: "guestsessiongroups",
                column: "table_id");

            migrationBuilder.CreateIndex(
                name: "IX_guestsessions_group_id",
                table: "guestsessions",
                column: "group_id");

            migrationBuilder.CreateIndex(
                name: "IX_guestsessions_table_id",
                table: "guestsessions",
                column: "table_id");

            migrationBuilder.CreateIndex(
                name: "IX_menuitems_place_id",
                table: "menuitems",
                column: "place_id");

            migrationBuilder.CreateIndex(
                name: "IX_menuitems_product_id",
                table: "menuitems",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "IX_orders_customer_id",
                table: "orders",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "IX_orders_guest_session_id",
                table: "orders",
                column: "guest_session_id");

            migrationBuilder.CreateIndex(
                name: "IX_orders_table_id",
                table: "orders",
                column: "table_id");

            migrationBuilder.CreateIndex(
                name: "IX_places_business_id",
                table: "places",
                column: "business_id");

            migrationBuilder.CreateIndex(
                name: "IX_places_city_id",
                table: "places",
                column: "city_id");

            migrationBuilder.CreateIndex(
                name: "IX_productcategory_parentcategory_id",
                table: "productcategory",
                column: "parentcategory_id");

            migrationBuilder.CreateIndex(
                name: "IX_products_business_id",
                table: "products",
                column: "business_id");

            migrationBuilder.CreateIndex(
                name: "IX_products_category_id",
                table: "products",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "IX_productsperorder_menuitem_id",
                table: "productsperorder",
                column: "menuitem_id");

            migrationBuilder.CreateIndex(
                name: "IX_reviews_customer_id",
                table: "reviews",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "IX_staff_place_id",
                table: "staff",
                column: "place_id");

            migrationBuilder.CreateIndex(
                name: "IX_tables_place_id",
                table: "tables",
                column: "place_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "productsperorder");

            migrationBuilder.DropTable(
                name: "reviews");

            migrationBuilder.DropTable(
                name: "staff");

            migrationBuilder.DropTable(
                name: "menuitems");

            migrationBuilder.DropTable(
                name: "orders");

            migrationBuilder.DropTable(
                name: "products");

            migrationBuilder.DropTable(
                name: "customers");

            migrationBuilder.DropTable(
                name: "guestsessions");

            migrationBuilder.DropTable(
                name: "productcategory");

            migrationBuilder.DropTable(
                name: "guestsessiongroups");

            migrationBuilder.DropTable(
                name: "tables");

            migrationBuilder.DropTable(
                name: "places");

            migrationBuilder.DropTable(
                name: "businesses");

            migrationBuilder.DropTable(
                name: "cities");
        }
    }
}
