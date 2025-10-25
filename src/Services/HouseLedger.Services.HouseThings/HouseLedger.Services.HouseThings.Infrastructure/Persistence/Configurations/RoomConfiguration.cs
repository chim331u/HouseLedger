using HouseLedger.Services.HouseThings.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HouseLedger.Services.HouseThings.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for Room entity.
/// Maps to existing table: MM_HouseThingsRooms
/// </summary>
public class RoomConfiguration : IEntityTypeConfiguration<Room>
{
    public void Configure(EntityTypeBuilder<Room> builder)
    {
        // Map to old table name
        builder.ToTable("MM_HouseThingsRooms");

        // Primary key
        builder.HasKey(r => r.Id);

        // Properties
        builder.Property(r => r.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(r => r.Description)
            .HasMaxLength(500);

        builder.Property(r => r.Color)
            .HasMaxLength(50);

        builder.Property(r => r.Icon)
            .HasMaxLength(100);

        // Audit fields from AuditableEntity
        builder.Property(r => r.CreatedDate)
            .IsRequired();

        builder.Property(r => r.LastUpdatedDate)
            .IsRequired();

        builder.Property(r => r.IsActive)
            .IsRequired();

        builder.Property(r => r.Note)
            .HasMaxLength(1000);

        // Navigation properties
        builder.HasMany(r => r.HouseThings)
            .WithOne(h => h.Room)
            .HasForeignKey(h => h.RoomId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(r => r.Name);
        builder.HasIndex(r => r.IsActive);
    }
}
