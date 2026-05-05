using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Models.Inventory;

namespace Shared.Data.Configurations;

public sealed class InventoryItemConfiguration : IEntityTypeConfiguration<InventoryItem>
{
    public void Configure(EntityTypeBuilder<InventoryItem> entity)
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
    }
}
