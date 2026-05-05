using Microsoft.EntityFrameworkCore;
using Shared.Models.Inventory;

namespace Inventory.Worker.Data;

public static class InventoryDbInitializer
{
    public static async Task InitializeAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();

        await dbContext.Database.MigrateAsync();
        await SeedAsync(dbContext);
    }

    private static async Task SeedAsync(InventoryDbContext dbContext)
    {
        if (await dbContext.InventoryItems.AnyAsync())
        {
            return;
        }

        dbContext.InventoryItems.AddRange(
            new InventoryItem("SKU-001", "WH-01", "A1-01", onHandQuantity: 100, availableQuantity: 100),
            new InventoryItem("SKU-002", "WH-01", "A1-02", onHandQuantity: 50, availableQuantity: 50));

        await dbContext.SaveChangesAsync();
    }
}
