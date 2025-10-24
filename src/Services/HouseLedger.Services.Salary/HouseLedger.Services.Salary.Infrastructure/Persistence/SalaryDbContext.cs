using HouseLedger.Services.Salary.Domain.Entities;
using HouseLedger.Services.Salary.Infrastructure.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HouseLedger.Services.Salary.Infrastructure.Persistence;

/// <summary>
/// DbContext for Salary service.
/// Points to existing HouseLedger.db database with table mappings.
/// </summary>
public class SalaryDbContext : DbContext
{
    private readonly ILogger<SalaryDbContext>? _logger;

    public SalaryDbContext(DbContextOptions<SalaryDbContext> options)
        : base(options)
    {
    }

    public SalaryDbContext(
        DbContextOptions<SalaryDbContext> options,
        ILogger<SalaryDbContext> logger)
        : base(options)
    {
        _logger = logger;
        _logger.LogDebug("SalaryDbContext instance created");
    }

    // DbSet for Salary entities
    public DbSet<Domain.Entities.Salary> Salaries { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        _logger?.LogDebug("Configuring Salary entity mappings");

        // Apply entity configurations
        modelBuilder.ApplyConfiguration(new SalaryConfiguration());

        _logger?.LogDebug("Salary entity configurations applied successfully");
    }

    public override int SaveChanges()
    {
        _logger?.LogDebug("SaveChanges called on SalaryDbContext");
        UpdateAuditFields();

        try
        {
            var result = base.SaveChanges();
            _logger?.LogInformation("SaveChanges completed successfully. {ChangeCount} entities affected", result);
            return result;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error occurred while saving changes to Salary database");
            throw;
        }
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        _logger?.LogDebug("SaveChangesAsync called on SalaryDbContext");
        UpdateAuditFields();

        try
        {
            var result = await base.SaveChangesAsync(cancellationToken);
            _logger?.LogInformation("SaveChangesAsync completed successfully. {ChangeCount} entities affected", result);
            return result;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error occurred while saving changes to Salary database asynchronously");
            throw;
        }
    }

    /// <summary>
    /// Updates CreatedDate and LastUpdatedDate for entities being added or modified.
    /// </summary>
    private void UpdateAuditFields()
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified)
            .ToList();

        _logger?.LogDebug("UpdateAuditFields: Found {Count} entries to process", entries.Count);

        foreach (var entry in entries)
        {
            _logger?.LogDebug("Processing entry: Type={Type}, State={State}",
                entry.Entity.GetType().Name, entry.State);

            if (entry.Entity is HouseLedger.Core.Domain.Common.AuditableEntity entity)
            {
                var now = DateTime.UtcNow;

                if (entry.State == EntityState.Added)
                {
                    _logger?.LogDebug("Setting audit fields for new entity: CreatedDate={Date}, IsActive=true", now);
                    entity.CreatedDate = now;
                    entity.LastUpdatedDate = now;
                    entity.IsActive = true;
                }
                else if (entry.State == EntityState.Modified)
                {
                    _logger?.LogDebug("Updating LastUpdatedDate for modified entity: {Date}", now);
                    entity.LastUpdatedDate = now;

                    // Prevent CreatedDate from being modified
                    entry.Property(nameof(entity.CreatedDate)).IsModified = false;
                }
            }
            else
            {
                _logger?.LogWarning("Entity {Type} is not an AuditableEntity", entry.Entity.GetType().Name);
            }
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
}
