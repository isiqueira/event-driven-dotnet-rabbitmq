using Microsoft.EntityFrameworkCore;
using Shared.Data.Extensions;
using Shared.Models.Messaging;

namespace Order.Worker.Data;

public sealed class OrderWorkerDbContext(DbContextOptions<OrderWorkerDbContext> options) : DbContext(options)
{
    public DbSet<ProcessedMessage> ProcessedMessages => Set<ProcessedMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyProcessedMessageConfiguration();
    }
}
