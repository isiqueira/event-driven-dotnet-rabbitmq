using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Models.Inventory;

namespace Shared.Data.Configurations;

public sealed class InventoryReservationConfiguration : IEntityTypeConfiguration<InventoryReservation>
{
    public void Configure(EntityTypeBuilder<InventoryReservation> entity)
    {
        entity.ToTable("inventory_reservations");
        entity.HasKey(reservation => reservation.Id).HasName("pk_inventory_reservations");

        entity.Property(reservation => reservation.Id).HasColumnName("id");
        entity.Property(reservation => reservation.OrderId).HasColumnName("order_id");
        entity.Property(reservation => reservation.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();
        entity.Property(reservation => reservation.FailureReason)
            .HasColumnName("failure_reason")
            .HasMaxLength(500);
        entity.Property(reservation => reservation.CreatedAt).HasColumnName("created_at");
        entity.Property(reservation => reservation.UpdatedAt).HasColumnName("updated_at");
        entity.Property(reservation => reservation.DeductedAt).HasColumnName("deducted_at");

        entity.HasMany(reservation => reservation.Items)
            .WithOne()
            .HasForeignKey(item => item.ReservationId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasIndex(reservation => reservation.OrderId)
            .IsUnique()
            .HasDatabaseName("ux_inventory_reservations_order_id");
    }
}
