using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Models.Inventory;

namespace Shared.Data.Configurations;

public sealed class InventoryReservationItemConfiguration : IEntityTypeConfiguration<InventoryReservationItem>
{
    public void Configure(EntityTypeBuilder<InventoryReservationItem> entity)
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
    }
}
