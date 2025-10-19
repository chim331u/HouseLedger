using HouseLedger.Services.Finance.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HouseLedger.Services.Finance.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for Card entity.
/// Maps to existing table: MM_CardMasterData
/// </summary>
public class CardConfiguration : IEntityTypeConfiguration<Card>
{
    public void Configure(EntityTypeBuilder<Card> builder)
    {
        // Map to old table name
        builder.ToTable("MM_CardMasterData");

        // Primary key
        builder.HasKey(c => c.Id);

        // Properties
        builder.Property(c => c.Name)
            .HasMaxLength(200);

        builder.Property(c => c.CardNumber)
            .HasMaxLength(20);

        builder.Property(c => c.CardType)
            .HasMaxLength(50);

        builder.Property(c => c.ExpirationDate)
            .IsRequired(false);

        builder.Property(c => c.CardholderName)
            .HasMaxLength(200);

        // Audit fields from AuditableEntity
        builder.Property(c => c.CreatedDate)
            .IsRequired();

        builder.Property(c => c.LastUpdatedDate)
            .IsRequired();

        builder.Property(c => c.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(c => c.Note)
            .HasMaxLength(1000);

        // Foreign key
        builder.Property(c => c.AccountId)
            .IsRequired(false);

        // Navigation property
        builder.HasOne(c => c.Account)
            .WithMany(a => a.Cards)
            .HasForeignKey(c => c.AccountId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(c => c.CardNumber);
        builder.HasIndex(c => c.AccountId);
        builder.HasIndex(c => c.IsActive);
    }
}
