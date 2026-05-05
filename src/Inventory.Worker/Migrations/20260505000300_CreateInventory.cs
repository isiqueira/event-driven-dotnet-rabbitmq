using Inventory.Worker.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Inventory.Worker.Migrations;

[DbContext(typeof(InventoryDbContext))]
[Migration("20260505000300_CreateInventory")]
public partial class CreateInventory : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "inventory_items",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                sku = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                warehouse_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                location_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                on_hand_quantity = table.Column<int>(type: "integer", nullable: false),
                available_quantity = table.Column<int>(type: "integer", nullable: false),
                reserved_quantity = table.Column<int>(type: "integer", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_inventory_items", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "inventory_processed_messages",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                event_id = table.Column<Guid>(type: "uuid", nullable: false),
                event_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                consumer = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                processed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                correlation_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_inventory_processed_messages", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "inventory_reservations",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                order_id = table.Column<Guid>(type: "uuid", nullable: false),
                status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                failure_reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                deducted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_inventory_reservations", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "inventory_reservation_items",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                reservation_id = table.Column<Guid>(type: "uuid", nullable: false),
                sku = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                quantity = table.Column<int>(type: "integer", nullable: false),
                warehouse_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                location_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_inventory_reservation_items", x => x.id);
                table.ForeignKey(
                    name: "fk_inventory_reservation_items_inventory_reservations_reservation_id",
                    column: x => x.reservation_id,
                    principalTable: "inventory_reservations",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "ux_inventory_items_sku_location",
            table: "inventory_items",
            columns: new[] { "sku", "warehouse_id", "location_id" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ux_inventory_processed_messages_event_consumer",
            table: "inventory_processed_messages",
            columns: new[] { "event_id", "event_type", "consumer" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_inventory_reservation_items_reservation_id",
            table: "inventory_reservation_items",
            column: "reservation_id");

        migrationBuilder.CreateIndex(
            name: "ux_inventory_reservations_order_id",
            table: "inventory_reservations",
            column: "order_id",
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "inventory_items");
        migrationBuilder.DropTable(name: "inventory_processed_messages");
        migrationBuilder.DropTable(name: "inventory_reservation_items");
        migrationBuilder.DropTable(name: "inventory_reservations");
    }
}
