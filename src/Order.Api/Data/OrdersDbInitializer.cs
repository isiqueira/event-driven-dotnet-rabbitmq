using Microsoft.EntityFrameworkCore;

namespace Order.Api.Data;

public static class OrdersDbInitializer
{
    public static async Task InitializeAsync(
        IServiceProvider services,
        CancellationToken cancellationToken = default)
    {
        using var scope = services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<OrdersDbContext>();
        await dbContext.Database.MigrateAsync(cancellationToken);
    }
}
