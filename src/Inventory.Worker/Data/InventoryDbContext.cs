using Inventory.Worker.Models;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Worker.Data;

public sealed class InventoryDbContext(DbContextOptions<InventoryDbContext> options) : DbContext(options)
{
    public DbSet<InventoryItem> InventoryItems => Set<InventoryItem>();
    public DbSet<InventoryReservation> InventoryReservations => Set<InventoryReservation>();
    public DbSet<InventoryReservationItem> InventoryReservationItems => Set<InventoryReservationItem>();
    public DbSet<ProcessedMessage> ProcessedMessages => Set<ProcessedMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<InventoryItem>(entity =>
        {
            entity.ToTable("inventory_items");
            entity.HasKey(item => item.Id).HasName("pk_inventory_items");

            entity.Property(item => item.Id).HasColumnName("id");
            entity.Property(item => item.Sku)
                .HasColumnName("sku")
                .HasMaxLength(100)
                .IsRequired();
            entity.Property(item => item.WarehouseId)
                .HasColumnName("warehouse_id")
                .HasMaxLength(64)
                .IsRequired();
            entity.Property(item => item.LocationId)
                .HasColumnName("location_id")
                .HasMaxLength(64)
                .IsRequired();
            entity.Property(item => item.OnHandQuantity).HasColumnName("on_hand_quantity");
            entity.Property(item => item.AvailableQuantity).HasColumnName("available_quantity");
            entity.Property(item => item.ReservedQuantity).HasColumnName("reserved_quantity");

            entity.HasIndex(item => new
                {
                    item.Sku,
                    item.WarehouseId,
                    item.LocationId
                })
                .IsUnique()
                .HasDatabaseName("ux_inventory_items_sku_location");
        });

        modelBuilder.Entity<InventoryReservation>(entity =>
        {
            entity.ToTable("inventory_reservations");
            entity.HasKey(reservation => reservation.Id).HasName("pk_inventory_reservations");

            entity.Property(reservation => reservation.Id).HasColumnName("id");
            entity.Property(reservation => reservation.OrderId).HasColumnName("order_id");
            entity.Property(reservation => reservation.Status)
                .HasColumnName("status")
                .HasConversion<string>()
                .HasMaxLength(32)
                .IsRequired();
            entity.Property(reservation => reservation.FailureReason)
                .HasColumnName("failure_reason")
                .HasMaxLength(500);
            entity.Property(reservation => reservation.CreatedAt).HasColumnName("created_at");
            entity.Property(reservation => reservation.UpdatedAt).HasColumnName("updated_at");
            entity.Property(reservation => reservation.DeductedAt).HasColumnName("deducted_at");

            entity.HasMany(reservation => reservation.Items)
                .WithOne()
                .HasForeignKey(item => item.ReservationId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(reservation => reservation.OrderId)
                .IsUnique()
                .HasDatabaseName("ux_inventory_reservations_order_id");
        });

        modelBuilder.Entity<InventoryReservationItem>(entity =>
        {
            entity.ToTable("inventory_reservation_items");
            entity.HasKey(item => item.Id).HasName("pk_inventory_reservation_items");

            entity.Property(item => item.Id).HasColumnName("id");
            entity.Property(item => item.ReservationId).HasColumnName("reservation_id");
            entity.Property(item => item.Sku)
                .HasColumnName("sku")
                .HasMaxLength(100)
                .IsRequired();
            entity.Property(item => item.Quantity).HasColumnName("quantity");
            entity.Property(item => item.WarehouseId)
                .HasColumnName("warehouse_id")
                .HasMaxLength(64)
                .IsRequired();
            entity.Property(item => item.LocationId)
                .HasColumnName("location_id")
                .HasMaxLength(64)
                .IsRequired();

            entity.HasIndex(item => item.ReservationId)
                .HasDatabaseName("ix_inventory_reservation_items_reservation_id");
        });

        modelBuilder.Entity<ProcessedMessage>(entity =>
        {
            entity.ToTable("inventory_processed_messages");
            entity.HasKey(message => message.Id).HasName("pk_inventory_processed_messages");

            entity.Property(message => message.Id).HasColumnName("id");
            entity.Property(message => message.EventId).HasColumnName("event_id");
            entity.Property(message => message.EventType)
                .HasColumnName("event_type")
                .HasMaxLength(100)
                .IsRequired();
            entity.Property(message => message.Consumer)
                .HasColumnName("consumer")
                .HasMaxLength(100)
                .IsRequired();
            entity.Property(message => message.ProcessedAt).HasColumnName("processed_at");
            entity.Property(message => message.CorrelationId)
                .HasColumnName("correlation_id")
                .HasMaxLength(100)
                .IsRequired();

            entity.HasIndex(message => new
                {
                    message.EventId,
                    message.EventType,
                    message.Consumer
                })
                .IsUnique()
                .HasDatabaseName("ux_inventory_processed_messages_event_consumer");
        });
    }
}
