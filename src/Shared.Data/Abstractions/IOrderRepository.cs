using Shared.Models.Orders;

namespace Shared.Data.Abstractions;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(Guid orderId, CancellationToken cancellationToken);
    Task<IReadOnlyList<Order>> ListAsync(CancellationToken cancellationToken);
    Task AddAsync(Order order, CancellationToken cancellationToken);
    Task UpdateStatusAsync(Guid orderId, OrderStatus status, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
