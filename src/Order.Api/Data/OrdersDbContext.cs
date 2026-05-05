using Microsoft.EntityFrameworkCore;
using Shared.Data.Extensions;
using Shared.Models.Orders;
using OrderEntity = Shared.Models.Orders.Order;

namespace Order.Api.Data;

public sealed class OrdersDbContext(DbContextOptions<OrdersDbContext> options) : DbContext(options)
{
    public DbSet<OrderEntity> Orders => Set<OrderEntity>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyOrderConfigurations();
    }
}
