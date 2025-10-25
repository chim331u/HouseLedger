using HouseLedger.Services.Finance.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HouseLedger.Services.Finance.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for Balance entity.
/// Maps to existing table: MM_Balance
/// </summary>
public class BalanceConfiguration : IEntityTypeConfiguration<Balance>
{
    public void Configure(EntityTypeBuilder<Balance> builder)
    {
        // Map to old table name
        builder.ToTable("MM_Balance");

        // Primary key
        builder.HasKey(b => b.Id);

        // Properties with column mappings to old names
        builder.Property(b => b.Amount)
            .HasColumnName("BalanceValue")  // Old column name
            .IsRequired();

        builder.Property(b => b.BalanceDate)
            .HasColumnName("DateBalance")  // Old column name
            .IsRequired();

        // Audit fields from AuditableEntity
        builder.Property(b => b.CreatedDate)
            .IsRequired();

        builder.Property(b => b.LastUpdatedDate)
            .IsRequired();

        builder.Property(b => b.IsActive)
            .IsRequired();

        builder.Property(b => b.Note)
            .HasMaxLength(1000);

        // Foreign key
        builder.Property(b => b.AccountId)
            .IsRequired(false);

        // Navigation property
        builder.HasOne(b => b.Account)
            .WithMany(a => a.Balances)
            .HasForeignKey(b => b.AccountId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(b => b.BalanceDate);
        builder.HasIndex(b => b.AccountId);
        builder.HasIndex(b => b.IsActive);
    }
}
