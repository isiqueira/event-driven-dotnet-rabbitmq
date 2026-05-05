using Microsoft.EntityFrameworkCore;
using Order.Worker.Models;

namespace Order.Worker.Data;

public sealed class WorkerDbContext(DbContextOptions<WorkerDbContext> options) : DbContext(options)
{
    public DbSet<ProcessedMessage> ProcessedMessages => Set<ProcessedMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProcessedMessage>(entity =>
        {
            entity.ToTable("processed_messages");
            entity.HasKey(message => message.Id).HasName("pk_processed_messages");

            entity.Property(message => message.Id).HasColumnName("id");
            entity.Property(message => message.EventId).HasColumnName("event_id");
            entity.Property(message => message.EventType)
                .HasColumnName("event_type")
                .HasMaxLength(100)
                .IsRequired();
            entity.Property(message => message.Consumer)
                .HasColumnName("consumer")
                .HasMaxLength(100)
                .IsRequired();
            entity.Property(message => message.ProcessedAt).HasColumnName("processed_at");
            entity.Property(message => message.CorrelationId)
                .HasColumnName("correlation_id")
                .HasMaxLength(100)
                .IsRequired();

            entity.HasIndex(message => new
                {
                    message.EventId,
                    message.EventType,
                    message.Consumer
                })
                .IsUnique()
                .HasDatabaseName("ux_processed_messages_event_consumer");
        });
    }
}
