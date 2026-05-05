using Microsoft.EntityFrameworkCore;

namespace Order.Worker.Data;

public static class WorkerDbInitializer
{
    public static async Task InitializeAsync(
        IServiceProvider services,
        CancellationToken cancellationToken = default)
    {
        using var scope = services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<WorkerDbContext>();
        await dbContext.Database.MigrateAsync(cancellationToken);
    }
}
