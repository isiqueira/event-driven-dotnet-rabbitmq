using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Models.Messaging;

namespace Shared.Data.Configurations;

public sealed class ProcessedMessageConfiguration(
    string tableName = "processed_messages",
    string primaryKeyName = "pk_processed_messages",
    string eventConsumerIndexName = "ux_processed_messages_event_consumer")
    : IEntityTypeConfiguration<ProcessedMessage>
{
    public void Configure(EntityTypeBuilder<ProcessedMessage> entity)
    {
        entity.ToTable(tableName);
        entity.HasKey(message => message.Id).HasName(primaryKeyName);

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
            .HasDatabaseName(eventConsumerIndexName);
    }
}
