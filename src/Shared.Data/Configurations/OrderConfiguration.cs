using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Models.Orders;

namespace Shared.Data.Configurations;

public sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> entity)
    {
        entity.ToTable("orders");
        entity.HasKey(order => order.Id).HasName("pk_orders");

        entity.Property(order => order.Id).HasColumnName("id");
        entity.Property(order => order.CustomerId)
            .HasColumnName("customer_id")
            .HasMaxLength(100)
            .IsRequired();
        entity.Property(order => order.TotalAmount)
            .HasColumnName("total_amount")
            .HasPrecision(18, 2);
        entity.Property(order => order.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();
        entity.Property(order => order.CreatedAt).HasColumnName("created_at");
        entity.Property(order => order.UpdatedAt).HasColumnName("updated_at");

        entity.HasMany(order => order.Items)
            .WithOne()
            .HasForeignKey(item => item.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
