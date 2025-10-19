using HouseLedger.Services.Finance.Domain.Entities;
using HouseLedger.Services.Finance.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HouseLedger.Services.Finance.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for Transaction entity.
/// Maps to existing table: TX_Transaction
/// Uses ComplexType for TransactionCategory Value Object (EF Core 8 feature)
/// </summary>
public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        // Map to old table name
        builder.ToTable("TX_Transaction");

        // Primary key
        builder.HasKey(t => t.Id);

        // Properties with column mappings to old names
        builder.Property(t => t.TransactionDate)
            .HasColumnName("TxnDate")  // Old column name
            .IsRequired();

        builder.Property(t => t.Amount)
            .HasColumnName("TxnAmount")  // Old column name
            .IsRequired();

        builder.Property(t => t.Description)
            .HasMaxLength(500);

        builder.Property(t => t.UniqueKey)
            .HasMaxLength(200);

        // Audit fields from AuditableEntity
        builder.Property(t => t.CreatedDate)
            .IsRequired();

        builder.Property(t => t.LastUpdatedDate)
            .IsRequired();

        builder.Property(t => t.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(t => t.Note)
            .HasMaxLength(1000);

        // Foreign key
        builder.Property(t => t.AccountId)
            .IsRequired();

        // Navigation property
        builder.HasOne(t => t.Account)
            .WithMany(a => a.Transactions)
            .HasForeignKey(t => t.AccountId)
            .OnDelete(DeleteBehavior.Restrict);

        // TransactionCategory Value Object - maps to Area + IsCatConfirmed columns
        // We use regular properties (Area, IsCatConfirmed) that back the Category Value Object
        builder.Property(t => t.Area)
            .HasMaxLength(100);

        builder.Property(t => t.IsCatConfirmed);

        // Ignore the Category computed property (it's calculated from Area + IsCatConfirmed)
        builder.Ignore(t => t.Category);

        // Indexes
        builder.HasIndex(t => t.TransactionDate);
        builder.HasIndex(t => t.AccountId);
        builder.HasIndex(t => t.IsActive);
        builder.HasIndex(t => t.UniqueKey);
    }
}
