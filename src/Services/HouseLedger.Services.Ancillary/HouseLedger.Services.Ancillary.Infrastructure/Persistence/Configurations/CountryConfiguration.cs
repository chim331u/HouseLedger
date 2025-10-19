using HouseLedger.Services.Ancillary.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HouseLedger.Services.Ancillary.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for Country entity.
/// Maps to table: AD_Country
/// </summary>
public class CountryConfiguration : IEntityTypeConfiguration<Country>
{
    public void Configure(EntityTypeBuilder<Country> builder)
    {
        // Table mapping
        builder.ToTable("AD_Country");

        // Primary key
        builder.HasKey(c => c.Id);

        // Properties
        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.Description)
            .HasMaxLength(500);

        builder.Property(c => c.CountryCodeAlf3)
            .IsRequired()
            .HasMaxLength(3);

        builder.Property(c => c.CountryCodeNum3)
            .HasMaxLength(3);

        // Audit fields (from AuditableEntity)
        builder.Property(c => c.CreatedDate)
            .IsRequired();

        builder.Property(c => c.LastUpdatedDate)
            .IsRequired();

        builder.Property(c => c.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(c => c.Note)
            .HasMaxLength(1000);
    }
}
