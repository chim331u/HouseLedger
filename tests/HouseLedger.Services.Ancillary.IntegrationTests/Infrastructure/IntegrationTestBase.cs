using HouseLedger.Services.Ancillary.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;

namespace HouseLedger.Services.Ancillary.IntegrationTests.Infrastructure;

/// <summary>
/// Base class for integration tests that provides a real SQLite database.
/// Uses SQLite in-memory mode for fast, isolated tests.
/// </summary>
public abstract class IntegrationTestBase : IDisposable
{
    private readonly SqliteConnection _connection;
    protected readonly AncillaryDbContext Context;

    protected IntegrationTestBase()
    {
        // Create and open a connection to SQLite in-memory database
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        // Configure DbContext to use the SQLite connection
        var options = new DbContextOptionsBuilder<AncillaryDbContext>()
            .UseSqlite(_connection)
            .Options;

        Context = new AncillaryDbContext(options);

        // Create the database schema
        Context.Database.EnsureCreated();
    }

    public void Dispose()
    {
        Context.Dispose();
        _connection.Close();
        _connection.Dispose();
    }
}
