using HouseLedger.Services.Ancillary.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HouseLedger.Services.Ancillary.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for CurrencyConversionRate entity.
/// Maps to table: AD_CurrencyConversionRate
/// </summary>
public class CurrencyConversionRateConfiguration : IEntityTypeConfiguration<CurrencyConversionRate>
{
    public void Configure(EntityTypeBuilder<CurrencyConversionRate> builder)
    {
        // Table mapping
        builder.ToTable("AD_CurrencyConversionRate");

        // Primary key
        builder.HasKey(c => c.Id);

        // Properties
        builder.Property(c => c.RateValue)
            .IsRequired()
            .HasPrecision(18, 6); // Allow for precise currency conversion rates

        builder.Property(c => c.CurrencyCodeAlf3)
            .IsRequired()
            .HasMaxLength(3);

        builder.Property(c => c.ReferringDate)
            .IsRequired();

        builder.Property(c => c.UniqueKey)
            .HasMaxLength(50);

        // Index for unique key (deduplication)
        builder.HasIndex(c => c.UniqueKey)
            .IsUnique()
            .HasFilter("UniqueKey IS NOT NULL");

        // Index for currency code and date lookups
        builder.HasIndex(c => new { c.CurrencyCodeAlf3, c.ReferringDate });

        // Audit fields (from AuditableEntity)
        builder.Property(c => c.CreatedDate)
            .IsRequired();

        builder.Property(c => c.LastUpdatedDate)
            .IsRequired();

        builder.Property(c => c.IsActive)
            .IsRequired();

        builder.Property(c => c.Note)
            .HasMaxLength(1000);
    }
}
