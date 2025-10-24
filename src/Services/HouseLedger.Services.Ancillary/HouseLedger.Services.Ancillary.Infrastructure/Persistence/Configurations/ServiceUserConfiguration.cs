using HouseLedger.Services.Ancillary.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HouseLedger.Services.Ancillary.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for ServiceUser entity.
/// Maps to table: AD_ServiceUser
/// </summary>
public class ServiceUserConfiguration : IEntityTypeConfiguration<ServiceUser>
{
    public void Configure(EntityTypeBuilder<ServiceUser> builder)
    {
        // Table mapping
        builder.ToTable("AD_ServiceUser");

        // Primary key
        builder.HasKey(su => su.Id);

        // Properties
        builder.Property(su => su.Name)
            .HasMaxLength(100);

        builder.Property(su => su.Surname)
            .HasMaxLength(100);

        // Audit fields (from AuditableEntity)
        builder.Property(su => su.CreatedDate)
            .IsRequired();

        builder.Property(su => su.LastUpdatedDate)
            .IsRequired();

        builder.Property(su => su.IsActive)
            .IsRequired();

        builder.Property(su => su.Note)
            .HasMaxLength(1000);

        // Indexes for performance
        builder.HasIndex(su => su.Surname)
            .HasDatabaseName("IX_AD_ServiceUser_Surname");

        builder.HasIndex(su => su.IsActive)
            .HasDatabaseName("IX_AD_ServiceUser_IsActive");
    }
}
