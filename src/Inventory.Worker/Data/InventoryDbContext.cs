using Microsoft.EntityFrameworkCore;
using Shared.Data.Extensions;
using Shared.Models.Inventory;
using Shared.Models.Messaging;

namespace Inventory.Worker.Data;

public sealed class InventoryDbContext(DbContextOptions<InventoryDbContext> options) : DbContext(options)
{
    public DbSet<InventoryItem> InventoryItems => Set<InventoryItem>();
    public DbSet<InventoryReservation> InventoryReservations => Set<InventoryReservation>();
    public DbSet<InventoryReservationItem> InventoryReservationItems => Set<InventoryReservationItem>();
    public DbSet<ProcessedMessage> ProcessedMessages => Set<ProcessedMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyInventoryConfigurations();
        modelBuilder.ApplyProcessedMessageConfiguration(
            tableName: "inventory_processed_messages",
            primaryKeyName: "pk_inventory_processed_messages",
            eventConsumerIndexName: "ux_inventory_processed_messages_event_consumer");
    }
}
