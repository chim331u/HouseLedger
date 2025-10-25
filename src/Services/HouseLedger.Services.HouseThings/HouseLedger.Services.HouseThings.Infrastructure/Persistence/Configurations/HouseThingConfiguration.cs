using HouseLedger.Services.HouseThings.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HouseLedger.Services.HouseThings.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for HouseThing entity.
/// Maps to existing table: MM_HouseThings
/// </summary>
public class HouseThingConfiguration : IEntityTypeConfiguration<HouseThing>
{
    public void Configure(EntityTypeBuilder<HouseThing> builder)
    {
        // Map to old table name
        builder.ToTable("MM_HouseThings");

        // Primary key
        builder.HasKey(h => h.Id);

        // Properties
        builder.Property(h => h.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(h => h.Description)
            .HasMaxLength(500);

        builder.Property(h => h.ItemType)
            .HasMaxLength(100);

        builder.Property(h => h.Model)
            .HasMaxLength(200);

        builder.Property(h => h.Cost)
            .IsRequired();

        builder.Property(h => h.HistoryId)
            .IsRequired();

        builder.Property(h => h.PurchaseDate)
            .IsRequired();

        // Foreign keys
        builder.Property(h => h.RoomId)
            .IsRequired(false);

        // Audit fields from AuditableEntity
        builder.Property(h => h.CreatedDate)
            .IsRequired();

        builder.Property(h => h.LastUpdatedDate)
            .IsRequired();

        builder.Property(h => h.IsActive)
            .IsRequired();

        builder.Property(h => h.Note)
            .HasMaxLength(1000);

        // Navigation properties
        builder.HasOne(h => h.Room)
            .WithMany(r => r.HouseThings)
            .HasForeignKey(h => h.RoomId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(h => h.Name);
        builder.HasIndex(h => h.HistoryId);
        builder.HasIndex(h => h.RoomId);
        builder.HasIndex(h => h.IsActive);
        builder.HasIndex(h => h.PurchaseDate);
    }
}
