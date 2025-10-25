using HouseLedger.Services.Finance.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HouseLedger.Services.Finance.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for Account entity.
/// Maps to existing table: MM_AccountMasterData
/// </summary>
public class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        // Map to old table name
        builder.ToTable("MM_AccountMasterData");

        // Primary key
        builder.HasKey(a => a.Id);

        // Properties with column mappings
        builder.Property(a => a.Name)
            .HasMaxLength(200);

        builder.Property(a => a.AccountNumber)
            .HasColumnName("Conto")  // Old column name
            .HasMaxLength(100);

        builder.Property(a => a.Description)
            .HasMaxLength(500);

        builder.Property(a => a.Iban)
            .HasMaxLength(50);

        builder.Property(a => a.Bic)
            .HasMaxLength(20);

        builder.Property(a => a.AccountType)
            .HasMaxLength(100);

        // Audit fields from AuditableEntity
        builder.Property(a => a.CreatedDate)
            .IsRequired();

        builder.Property(a => a.LastUpdatedDate)
            .IsRequired();

        builder.Property(a => a.IsActive)
            .IsRequired();

        builder.Property(a => a.Note)
            .HasMaxLength(1000);

        // Foreign keys
        builder.Property(a => a.CurrencyId)
            .IsRequired(false);

        builder.Property(a => a.BankId)
            .HasColumnName("BankMasterDataId")  // Old column name
            .IsRequired(false);

        // Navigation properties
        builder.HasOne(a => a.Bank)
            .WithMany(b => b.Accounts)
            .HasForeignKey(a => a.BankId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(a => a.Transactions)
            .WithOne(t => t.Account)
            .HasForeignKey(t => t.AccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(a => a.Balances)
            .WithOne(b => b.Account)
            .HasForeignKey(b => b.AccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(a => a.Cards)
            .WithOne(c => c.Account)
            .HasForeignKey(c => c.AccountId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(a => a.Name);
        builder.HasIndex(a => a.Iban);
        builder.HasIndex(a => a.IsActive);
        builder.HasIndex(a => a.BankId);
    }
}
