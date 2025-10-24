using HouseLedger.Services.Salary.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HouseLedger.Services.Salary.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework configuration for Salary entity.
/// Maps to existing "SL_Salary" table in the database.
/// </summary>
public class SalaryConfiguration : IEntityTypeConfiguration<Domain.Entities.Salary>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.Salary> builder)
    {
        // Table mapping (maps to existing legacy table)
        builder.ToTable("SL_Salary");

        // Primary Key
        builder.HasKey(s => s.Id);

        // Properties
        builder.Property(s => s.SalaryValue)
            .IsRequired()
            .HasColumnType("REAL"); // SQLite uses REAL for decimal

        builder.Property(s => s.SalaryValueEur)
            .IsRequired()
            .HasColumnType("REAL");

        builder.Property(s => s.SalaryDate)
            .IsRequired()
            .HasColumnType("TEXT"); // SQLite stores DateTime as TEXT

        builder.Property(s => s.ReferYear)
            .HasMaxLength(4)
            .HasColumnType("TEXT");

        builder.Property(s => s.ReferMonth)
            .HasMaxLength(20)
            .HasColumnType("TEXT");

        builder.Property(s => s.FileName)
            .HasMaxLength(255)
            .HasColumnType("TEXT");

        builder.Property(s => s.ExchangeRate)
            .IsRequired()
            .HasColumnName("ExcengeRate") // Note: Database has typo - "ExcengeRate" instead of "ExchangeRate"
            .HasColumnType("TEXT");

        builder.Property(s => s.CurrencyId)
            .HasColumnType("INTEGER");

        builder.Property(s => s.UserId)
            .HasColumnType("INTEGER");

        builder.Property(s => s.Note)
            .HasColumnType("TEXT");

        // Audit fields (from AuditableEntity)
        builder.Property(s => s.CreatedDate)
            .IsRequired()
            .HasColumnType("TEXT");

        builder.Property(s => s.LastUpdatedDate)
            .IsRequired()
            .HasColumnType("TEXT");

        builder.Property(s => s.IsActive)
            .IsRequired()
            .HasColumnType("INTEGER"); // SQLite uses INTEGER for bool (0/1)

        // Indexes for performance
        builder.HasIndex(s => s.UserId)
            .HasDatabaseName("IX_SL_Salary_UserId");

        builder.HasIndex(s => s.CurrencyId)
            .HasDatabaseName("IX_SL_Salary_CurrencyId");

        builder.HasIndex(s => s.SalaryDate)
            .HasDatabaseName("IX_SL_Salary_SalaryDate");

        builder.HasIndex(s => s.IsActive)
            .HasDatabaseName("IX_SL_Salary_IsActive");

        // Note: Navigation properties to Currency and User will be configured
        // when ServiceUser is added to the Ancillary service
    }
}
