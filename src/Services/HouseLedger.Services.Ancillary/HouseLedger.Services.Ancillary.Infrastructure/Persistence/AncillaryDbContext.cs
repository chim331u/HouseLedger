using HouseLedger.Services.Ancillary.Domain.Entities;
using HouseLedger.Services.Ancillary.Infrastructure.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HouseLedger.Services.Ancillary.Infrastructure.Persistence;

/// <summary>
/// DbContext for Ancillary service (Currency and Country reference data).
/// Points to existing MM.db database with table mappings to old schema.
/// </summary>
public class AncillaryDbContext : DbContext
{
    private readonly ILogger<AncillaryDbContext>? _logger;

    public AncillaryDbContext(DbContextOptions<AncillaryDbContext> options)
        : base(options)
    {
    }

    public AncillaryDbContext(
        DbContextOptions<AncillaryDbContext> options,
        ILogger<AncillaryDbContext> logger)
        : base(options)
    {
        _logger = logger;
        _logger.LogDebug("AncillaryDbContext instance created");
    }

    // DbSets for Ancillary entities
    public DbSet<Currency> Currencies { get; set; } = null!;
    public DbSet<Country> Countries { get; set; } = null!;
    public DbSet<CurrencyConversionRate> CurrencyConversionRates { get; set; } = null!;
    public DbSet<Supplier> Suppliers { get; set; } = null!;
    public DbSet<ServiceUser> ServiceUsers { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        _logger?.LogDebug("Configuring Ancillary entity mappings");

        // Apply entity configurations
        modelBuilder.ApplyConfiguration(new CurrencyConfiguration());
        modelBuilder.ApplyConfiguration(new CountryConfiguration());
        modelBuilder.ApplyConfiguration(new CurrencyConversionRateConfiguration());
        modelBuilder.ApplyConfiguration(new SupplierConfiguration());
        modelBuilder.ApplyConfiguration(new ServiceUserConfiguration());

        _logger?.LogDebug("Ancillary entity configurations applied successfully");
    }

    public override int SaveChanges()
    {
        _logger?.LogDebug("SaveChanges called on AncillaryDbContext");
        UpdateAuditFields();

        try
        {
            var result = base.SaveChanges();
            _logger?.LogInformation("SaveChanges completed successfully. {ChangeCount} entities affected", result);
            return result;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error occurred while saving changes to Ancillary database");
            throw;
        }
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        _logger?.LogDebug("SaveChangesAsync called on AncillaryDbContext");
        UpdateAuditFields();

        try
        {
            
            
            var result = await base.SaveChangesAsync(cancellationToken);
            _logger?.LogInformation("SaveChangesAsync completed successfully. {ChangeCount} entities affected", result);
            return result;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error occurred while saving changes to Ancillary database asynchronously");
            throw;
        }
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);

        // Enable sensitive data logging only in debug builds
#if DEBUG
        optionsBuilder.EnableSensitiveDataLogging();
        _logger?.LogDebug("Sensitive data logging enabled (Debug build)");
#endif
    }

    /// <summary>
    /// Updates CreatedDate and LastUpdatedDate for entities being added or modified.
    /// </summary>
    private void UpdateAuditFields()
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.Entity is HouseLedger.Core.Domain.Common.AuditableEntity &&
                       (e.State == EntityState.Added || e.State == EntityState.Modified));

        foreach (var entry in entries)
        {
            var entity = (HouseLedger.Core.Domain.Common.AuditableEntity)entry.Entity;

            if (entry.State == EntityState.Added)
            {
                entity.CreatedDate = DateTime.UtcNow;
                entity.LastUpdatedDate = DateTime.UtcNow;
                entity.IsActive = true;
            }
            else if (entry.State == EntityState.Modified)
            {
                entity.LastUpdatedDate = DateTime.UtcNow;
            }
        }
    }
}
