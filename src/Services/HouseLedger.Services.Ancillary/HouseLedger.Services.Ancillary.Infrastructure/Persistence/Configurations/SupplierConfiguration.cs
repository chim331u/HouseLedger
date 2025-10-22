using HouseLedger.Services.Ancillary.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HouseLedger.Services.Ancillary.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for Supplier entity.
/// Maps to table: Suppliers
/// </summary>
public class SupplierConfiguration : IEntityTypeConfiguration<Supplier>
{
    public void Configure(EntityTypeBuilder<Supplier> builder)
    {
        // Table mapping
        builder.ToTable("Suppliers");

        // Primary key
        builder.HasKey(s => s.Id);

        // Properties
        builder.Property(s => s.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(s => s.UnitMeasure)
            .HasMaxLength(50);

        builder.Property(s => s.Description)
            .HasMaxLength(500);

        builder.Property(s => s.Type)
            .HasMaxLength(100);

        builder.Property(s => s.Contract)
            .HasMaxLength(200);

        // Audit fields (from AuditableEntity)
        builder.Property(s => s.CreatedDate)
            .IsRequired();

        builder.Property(s => s.LastUpdatedDate)
            .IsRequired();

        builder.Property(s => s.IsActive)
            .IsRequired();

        builder.Property(s => s.Note)
            .HasMaxLength(1000);
    }
}
