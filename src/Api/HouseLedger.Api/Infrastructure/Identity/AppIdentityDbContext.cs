using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HouseLedger.Api.Infrastructure.Identity;

/// <summary>
/// Identity DbContext for HouseLedger application.
/// Maps to existing AspNetUsers tables in MM.db from the legacy mm_restapi system.
/// Uses the standard ASP.NET Core Identity schema (AspNetUsers, AspNetUserClaims, etc.).
/// </summary>
public class AppIdentityDbContext : IdentityUserContext<IdentityUser>
{
    private readonly ILogger<AppIdentityDbContext>? _logger;

    public AppIdentityDbContext(DbContextOptions<AppIdentityDbContext> options)
        : base(options)
    {
    }

    public AppIdentityDbContext(
        DbContextOptions<AppIdentityDbContext> options,
        ILogger<AppIdentityDbContext> logger)
        : base(options)
    {
        _logger = logger;
        _logger.LogDebug("AppIdentityDbContext instance created");
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        _logger?.LogDebug("Configuring Identity entity mappings");

        // The base class already configures the standard Identity tables:
        // - AspNetUsers
        // - AspNetUserClaims
        // - AspNetUserLogins
        // - AspNetUserTokens
        //
        // These tables already exist in MM.db from the legacy system,
        // so no additional configuration is needed.
        //
        // Note: We're using IdentityUserContext<IdentityUser> (not IdentityDbContext)
        // because we don't need roles in this application (simpler setup).
        // If roles are needed later, migrate to IdentityDbContext<IdentityUser, IdentityRole, string>.

        _logger?.LogDebug("Identity entity configurations applied successfully");
    }

    public override int SaveChanges()
    {
        _logger?.LogDebug("SaveChanges called on AppIdentityDbContext");
        try
        {
            var result = base.SaveChanges();
            _logger?.LogInformation("SaveChanges completed successfully. {ChangeCount} entities affected", result);
            return result;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error occurred while saving changes to Identity database");
            throw;
        }
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        _logger?.LogDebug("SaveChangesAsync called on AppIdentityDbContext");
        try
        {
            var result = await base.SaveChangesAsync(cancellationToken);
            _logger?.LogInformation("SaveChangesAsync completed successfully. {ChangeCount} entities affected", result);
            return result;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error occurred while saving changes to Identity database asynchronously");
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
