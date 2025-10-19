using HouseLedger.Services.Finance.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Formatting.Compact;

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File(
        new CompactJsonFormatter(),
        path: "logs/testconsole-.json",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 7)
    .CreateLogger();

Log.Information("HouseLedger - Finance Domain Test starting");

Console.WriteLine("HouseLedger - Finance Domain Test");
Console.WriteLine("==================================\n");

// Database path - adjust this to point to your actual MM.db file
var dbPath = "/Users/luca/GitHub/HouseLedger/temp/MM.db";

// Check if database exists
if (!File.Exists(dbPath))
{
    Log.Warning("Database not found at: {DatabasePath}", dbPath);
    Console.WriteLine($"⚠️  Database not found at: {dbPath}");
    Console.WriteLine("\nPlease update the dbPath variable in Program.cs to point to your MM.db file");
    Console.WriteLine("Example: var dbPath = \"/Users/luca/GitHub/HouseLedger/temp/MM.db\";");
    Log.CloseAndFlush();
    return;
}

Log.Information("Database found at: {DatabasePath}", dbPath);
Console.WriteLine($"✅ Database found at: {dbPath}\n");

// Create LoggerFactory
using var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.AddSerilog(dispose: true);
});

// Create DbContext with logger
var options = new DbContextOptionsBuilder<FinanceDbContext>()
    .UseSqlite($"Data Source={dbPath}")
    .UseLoggerFactory(loggerFactory)
    .Options;

var logger = loggerFactory.CreateLogger<FinanceDbContext>();
using var context = new FinanceDbContext(options, logger);

Log.Information("FinanceDbContext created and ready to use");

try
{
    // Test 1: Query Banks
    Log.Information("Starting Test 1: Querying Banks");
    Console.WriteLine("📊 Test 1: Querying Banks...");
    var banks = await context.Banks
        .Where(b => b.IsActive)
        .Take(5)
        .ToListAsync();

    Log.Information("Found {BankCount} active banks", banks.Count);
    Console.WriteLine($"   Found {banks.Count} active banks");
    foreach (var bank in banks)
    {
        Console.WriteLine($"   - {bank.Name} (ID: {bank.Id})");
    }
    Console.WriteLine();

    // Test 2: Query Accounts
    Log.Information("Starting Test 2: Querying Accounts");
    Console.WriteLine("💳 Test 2: Querying Accounts...");
    var accounts = await context.Accounts
        .Include(a => a.Bank)
        .Where(a => a.IsActive)
        .Take(5)
        .ToListAsync();

    Log.Information("Found {AccountCount} active accounts", accounts.Count);
    Console.WriteLine($"   Found {accounts.Count} active accounts");
    foreach (var account in accounts)
    {
        Console.WriteLine($"   - {account.Name} (Bank: {account.Bank?.Name ?? "N/A"})");
    }
    Console.WriteLine();

    // Test 3: Query Transactions with TransactionCategory Value Object
    Log.Information("Starting Test 3: Querying Transactions with TransactionCategory");
    Console.WriteLine("💰 Test 3: Querying Transactions (with TransactionCategory)...");
    var transactions = await context.Transactions
        .Include(t => t.Account)
        .Where(t => t.IsActive)
        .OrderByDescending(t => t.TransactionDate)
        .Take(10)
        .ToListAsync();

    Log.Information("Found {TransactionCount} recent transactions", transactions.Count);
    Console.WriteLine($"   Found {transactions.Count} recent transactions");
    foreach (var txn in transactions)
    {
        var category = txn.Category != null
            ? $"{txn.Category.Name} ({(txn.Category.IsConfirmed ? "✓" : "?")})"
            : "No category";

        Console.WriteLine($"   - {txn.TransactionDate:yyyy-MM-dd} | {txn.Amount:F2} | {category}");
        Console.WriteLine($"     Account: {txn.Account?.Name ?? "Unknown"}");
        if (!string.IsNullOrEmpty(txn.Description))
        {
            Console.WriteLine($"     Desc: {txn.Description}");
        }
        Console.WriteLine();
    }

    // Test 4: Query Balances
    Log.Information("Starting Test 4: Querying Balances");
    Console.WriteLine("📈 Test 4: Querying Balances...");
    var balances = await context.Balances
        .Include(b => b.Account)
        .Where(b => b.IsActive)
        .OrderByDescending(b => b.BalanceDate)
        .Take(5)
        .ToListAsync();

    Log.Information("Found {BalanceCount} recent balances", balances.Count);
    Console.WriteLine($"   Found {balances.Count} recent balances");
    foreach (var balance in balances)
    {
        Console.WriteLine($"   - {balance.BalanceDate:yyyy-MM-dd} | {balance.Amount:F2} | Account: {balance.Account?.Name ?? "Unknown"}");
    }
    Console.WriteLine();

    // Test 5: Statistics
    Log.Information("Starting Test 5: Database Statistics");
    Console.WriteLine("📊 Test 5: Database Statistics...");
    var totalBanks = await context.Banks.CountAsync();
    var totalAccounts = await context.Accounts.CountAsync();
    var totalTransactions = await context.Transactions.CountAsync();
    var totalBalances = await context.Balances.CountAsync();
    var totalCards = await context.Cards.CountAsync();

    Log.Information("Database statistics: Banks={Banks}, Accounts={Accounts}, Transactions={Transactions}, Balances={Balances}, Cards={Cards}",
        totalBanks, totalAccounts, totalTransactions, totalBalances, totalCards);

    Console.WriteLine($"   Total Banks: {totalBanks}");
    Console.WriteLine($"   Total Accounts: {totalAccounts}");
    Console.WriteLine($"   Total Transactions: {totalTransactions}");
    Console.WriteLine($"   Total Balances: {totalBalances}");
    Console.WriteLine($"   Total Cards: {totalCards}");
    Console.WriteLine();

    Log.Information("All tests completed successfully");
    Console.WriteLine("✅ All tests completed successfully!");
    Console.WriteLine("\n🎉 Finance Domain is working correctly with your existing database!");
}
catch (Exception ex)
{
    Log.Error(ex, "Error occurred during test execution");
    Console.WriteLine($"\n❌ Error: {ex.Message}");
    Console.WriteLine($"\nStack trace:\n{ex.StackTrace}");

    if (ex.InnerException != null)
    {
        Console.WriteLine($"\nInner exception: {ex.InnerException.Message}");
    }
}
finally
{
    Log.Information("Test console application ending");
    Log.CloseAndFlush();
}
