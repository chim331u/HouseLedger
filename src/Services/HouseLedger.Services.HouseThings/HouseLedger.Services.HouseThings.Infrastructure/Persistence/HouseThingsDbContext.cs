using HouseLedger.Services.HouseThings.Domain.Entities;
using HouseLedger.Services.HouseThings.Infrastructure.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HouseLedger.Services.HouseThings.Infrastructure.Persistence;

/// <summary>
/// DbContext for HouseThings service.
/// Points to existing MM.db database with table mappings to old schema.
/// </summary>
public class HouseThingsDbContext : DbContext
{
    private readonly ILogger<HouseThingsDbContext>? _logger;

    public HouseThingsDbContext(DbContextOptions<HouseThingsDbContext> options)
        : base(options)
    {
    }

    public HouseThingsDbContext(
        DbContextOptions<HouseThingsDbContext> options,
        ILogger<HouseThingsDbContext> logger)
        : base(options)
    {
        _logger = logger;
        _logger.LogDebug("HouseThingsDbContext instance created");
    }

    // DbSets for HouseThings entities
    public DbSet<Room> Rooms { get; set; } = null!;
    public DbSet<HouseThing> HouseThings { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        _logger?.LogDebug("Configuring HouseThings entity mappings");

        // Apply entity configurations
        modelBuilder.ApplyConfiguration(new RoomConfiguration());
        modelBuilder.ApplyConfiguration(new HouseThingConfiguration());

        _logger?.LogDebug("HouseThings entity configurations applied successfully");
    }

    public override int SaveChanges()
    {
        _logger?.LogDebug("SaveChanges called on HouseThingsDbContext");
        UpdateAuditFields();

        try
        {
            var result = base.SaveChanges();
            _logger?.LogInformation("SaveChanges completed successfully. {ChangeCount} entities affected", result);
            return result;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error occurred while saving changes to HouseThings database");
            throw;
        }
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        _logger?.LogDebug("SaveChangesAsync called on HouseThingsDbContext");
        UpdateAuditFields();

        try
        {
            var result = await base.SaveChangesAsync(cancellationToken);
            _logger?.LogInformation("SaveChangesAsync completed successfully. {ChangeCount} entities affected", result);
            return result;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error occurred while saving changes to HouseThings database asynchronously");
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
