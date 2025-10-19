using HouseLedger.Services.Finance.Domain.Entities;
using HouseLedger.Services.Finance.Infrastructure.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HouseLedger.Services.Finance.Infrastructure.Persistence;

/// <summary>
/// DbContext for Finance service.
/// Points to existing MM.db database with table mappings to old schema.
/// </summary>
public class FinanceDbContext : DbContext
{
    private readonly ILogger<FinanceDbContext>? _logger;

    public FinanceDbContext(DbContextOptions<FinanceDbContext> options)
        : base(options)
    {
    }

    public FinanceDbContext(
        DbContextOptions<FinanceDbContext> options,
        ILogger<FinanceDbContext> logger)
        : base(options)
    {
        _logger = logger;
        _logger.LogDebug("FinanceDbContext instance created");
    }

    // DbSets for Finance entities
    public DbSet<Transaction> Transactions { get; set; } = null!;
    public DbSet<Account> Accounts { get; set; } = null!;
    public DbSet<Balance> Balances { get; set; } = null!;
    public DbSet<Bank> Banks { get; set; } = null!;
    public DbSet<Card> Cards { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        _logger?.LogDebug("Configuring Finance entity mappings");

        // Apply entity configurations
        modelBuilder.ApplyConfiguration(new TransactionConfiguration());
        modelBuilder.ApplyConfiguration(new AccountConfiguration());
        modelBuilder.ApplyConfiguration(new BalanceConfiguration());
        modelBuilder.ApplyConfiguration(new BankConfiguration());
        modelBuilder.ApplyConfiguration(new CardConfiguration());

        _logger?.LogDebug("Finance entity configurations applied successfully");
    }

    public override int SaveChanges()
    {
        _logger?.LogDebug("SaveChanges called on FinanceDbContext");
        try
        {
            var result = base.SaveChanges();
            _logger?.LogInformation("SaveChanges completed successfully. {ChangeCount} entities affected", result);
            return result;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error occurred while saving changes to Finance database");
            throw;
        }
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        _logger?.LogDebug("SaveChangesAsync called on FinanceDbContext");
        try
        {
            var result = await base.SaveChangesAsync(cancellationToken);
            _logger?.LogInformation("SaveChangesAsync completed successfully. {ChangeCount} entities affected", result);
            return result;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error occurred while saving changes to Finance database asynchronously");
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
}
