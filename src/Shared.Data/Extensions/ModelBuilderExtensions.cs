using Microsoft.EntityFrameworkCore;
using Shared.Data.Configurations;

namespace Shared.Data.Extensions;

public static class ModelBuilderExtensions
{
    public static ModelBuilder ApplyOrderConfigurations(this ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new OrderConfiguration());
        modelBuilder.ApplyConfiguration(new OrderItemConfiguration());

        return modelBuilder;
    }

    public static ModelBuilder ApplyInventoryConfigurations(this ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new InventoryItemConfiguration());
        modelBuilder.ApplyConfiguration(new InventoryReservationConfiguration());
        modelBuilder.ApplyConfiguration(new InventoryReservationItemConfiguration());

        return modelBuilder;
    }

    public static ModelBuilder ApplyProcessedMessageConfiguration(
        this ModelBuilder modelBuilder,
        string tableName = "processed_messages",
        string primaryKeyName = "pk_processed_messages",
        string eventConsumerIndexName = "ux_processed_messages_event_consumer")
    {
        modelBuilder.ApplyConfiguration(new ProcessedMessageConfiguration(
            tableName,
            primaryKeyName,
            eventConsumerIndexName));

        return modelBuilder;
    }
}
