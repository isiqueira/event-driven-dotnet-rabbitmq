using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Models.Orders;

namespace Shared.Data.Configurations;

public sealed class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> entity)
    {
        entity.ToTable("order_items");
        entity.HasKey(item => item.Id).HasName("pk_order_items");

        entity.Property(item => item.Id).HasColumnName("id");
        entity.Property(item => item.OrderId).HasColumnName("order_id");
        entity.Property(item => item.Sku)
            .HasColumnName("sku")
            .HasMaxLength(100)
            .IsRequired();
        entity.Property(item => item.Quantity).HasColumnName("quantity");
        entity.Property(item => item.UnitPrice)
            .HasColumnName("unit_price")
            .HasPrecision(18, 2);
        entity.Property(item => item.TotalPrice)
            .HasColumnName("total_price")
            .HasPrecision(18, 2);

        entity.HasIndex(item => item.OrderId).HasDatabaseName("ix_order_items_order_id");
    }
}
