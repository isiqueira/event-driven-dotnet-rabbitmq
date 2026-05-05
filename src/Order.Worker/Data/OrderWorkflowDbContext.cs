using Microsoft.EntityFrameworkCore;
using Order.Worker.Models;

namespace Order.Worker.Data;

public sealed class OrderWorkflowDbContext(DbContextOptions<OrderWorkflowDbContext> options) : DbContext(options)
{
    public DbSet<OrderWorkflowOrder> Orders => Set<OrderWorkflowOrder>();
    public DbSet<OrderWorkflowOrderItem> OrderItems => Set<OrderWorkflowOrderItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<OrderWorkflowOrder>(entity =>
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
                .HasForeignKey(item => item.OrderId);
        });

        modelBuilder.Entity<OrderWorkflowOrderItem>(entity =>
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
        });
    }
}
