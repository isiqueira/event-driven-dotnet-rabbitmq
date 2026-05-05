using Microsoft.EntityFrameworkCore;
using Shared.Data.Abstractions;
using Shared.Models.Orders;
using OrderEntity = Shared.Models.Orders.Order;

namespace Order.Worker.Data;

public sealed class EfOrderRepository(OrderWorkflowDbContext dbContext) : IOrderRepository
{
    public async Task<OrderEntity?> GetByIdAsync(Guid orderId, CancellationToken cancellationToken)
    {
        return await dbContext.Orders
            .Include(order => order.Items)
            .FirstOrDefaultAsync(order => order.Id == orderId, cancellationToken);
    }

    public async Task<IReadOnlyList<OrderEntity>> ListAsync(CancellationToken cancellationToken)
    {
        return await dbContext.Orders
            .Include(order => order.Items)
            .OrderByDescending(order => order.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(OrderEntity order, CancellationToken cancellationToken)
    {
        await dbContext.Orders.AddAsync(order, cancellationToken);
    }

    public async Task UpdateStatusAsync(Guid orderId, OrderStatus status, CancellationToken cancellationToken)
    {
        var order = await dbContext.Orders.FirstOrDefaultAsync(
            existingOrder => existingOrder.Id == orderId,
            cancellationToken);

        if (order is null)
        {
            return;
        }

        order.UpdateStatus(status, DateTimeOffset.UtcNow);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
