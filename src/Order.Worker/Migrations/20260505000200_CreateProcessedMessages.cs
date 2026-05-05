using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Order.Worker.Data;

#nullable disable

namespace Order.Worker.Migrations;

[DbContext(typeof(OrderWorkerDbContext))]
[Migration("20260505000200_CreateProcessedMessages")]
public partial class CreateProcessedMessages : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "processed_messages",
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
                table.PrimaryKey("pk_processed_messages", x => x.id);
            });

        migrationBuilder.CreateIndex(
            name: "ux_processed_messages_event_consumer",
            table: "processed_messages",
            columns: new[] { "event_id", "event_type", "consumer" },
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "processed_messages");
    }
}
