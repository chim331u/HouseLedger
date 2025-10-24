using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace HouseLedger.Services.Salary.Infrastructure.Persistence;

/// <summary>
/// Design-time factory for SalaryDbContext used by EF Core migrations.
/// </summary>
public class SalaryDbContextFactory : IDesignTimeDbContextFactory<SalaryDbContext>
{
    public SalaryDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<SalaryDbContext>();

        // Use SQLite with a connection string
        optionsBuilder.UseSqlite("Data Source=housledger.db");

        return new SalaryDbContext(optionsBuilder.Options);
    }
}
