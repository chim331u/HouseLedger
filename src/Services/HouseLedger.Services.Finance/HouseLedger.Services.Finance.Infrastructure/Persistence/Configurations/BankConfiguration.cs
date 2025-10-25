using HouseLedger.Services.Finance.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HouseLedger.Services.Finance.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for Bank entity.
/// Maps to existing table: MM_BankMasterData
/// </summary>
public class BankConfiguration : IEntityTypeConfiguration<Bank>
{
    public void Configure(EntityTypeBuilder<Bank> builder)
    {
        // Map to old table name
        builder.ToTable("MM_BankMasterData");

        // Primary key
        builder.HasKey(b => b.Id);

        // Properties
        builder.Property(b => b.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(b => b.Description)
            .HasMaxLength(50);

        builder.Property(b => b.WebUrl)
            .HasMaxLength(500);

        builder.Property(b => b.Address)
            .HasMaxLength(500);

        builder.Property(b => b.City)
            .HasMaxLength(200);

        builder.Property(b => b.Phone)
            .HasMaxLength(50);

        builder.Property(b => b.Mail)
            .HasMaxLength(200);

        builder.Property(b => b.ReferenceName)
            .HasMaxLength(200);

        // Audit fields from AuditableEntity
        builder.Property(b => b.CreatedDate)
            .IsRequired();

        builder.Property(b => b.LastUpdatedDate)
            .IsRequired();

        builder.Property(b => b.IsActive)
            .IsRequired();

        builder.Property(b => b.Note)
            .HasMaxLength(1000);

        // Foreign key to Country (in Ancillary service)
        // Note: We don't configure the navigation here since Country is in another service
        builder.Property(b => b.CountryId)
            .IsRequired(false);

        // Navigation properties
        builder.HasMany(b => b.Accounts)
            .WithOne(a => a.Bank)
            .HasForeignKey(a => a.BankId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(b => b.Name);
        builder.HasIndex(b => b.IsActive);
    }
}
