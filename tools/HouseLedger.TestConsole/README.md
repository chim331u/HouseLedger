# HouseLedger Test Console

This console app tests the Finance Domain with your existing `MM.db` database.

## Purpose

Validates that:
- FinanceDbContext connects to existing database
- Entity mappings to old tables work correctly
- TransactionCategory Value Object loads from old `Area` + `IsCatConfirmed` columns
- Navigation properties work (Account → Bank, Transaction → Account, etc.)

## How to Run

1. **Find your MM.db database file**
   - Check: `/data/MM.db`
   - Or search: `find ~ -name "MM.db" 2>/dev/null`

2. **Update database path in Program.cs**
   - Open: `tools/HouseLedger.TestConsole/Program.cs`
   - Line 8: `var dbPath = "/data/MM.db";`
   - Change to your actual path

3. **Run the test**
   ```bash
   dotnet run --project tools/HouseLedger.TestConsole/HouseLedger.TestConsole.csproj
   ```

## Expected Output

```
HouseLedger - Finance Domain Test
==================================

✅ Database found at: /data/MM.db

📊 Test 1: Querying Banks...
   Found 5 active banks
   - Bank Name 1 (ID: 1)
   - Bank Name 2 (ID: 2)
   ...

💳 Test 2: Querying Accounts...
   Found 5 active accounts
   - Account 1 (Bank: Bank Name)
   ...

💰 Test 3: Querying Transactions (with TransactionCategory)...
   Found 10 recent transactions
   - 2024-10-15 | 123.45 | Groceries (✓)
     Account: My Checking
     Desc: Weekly shopping
   ...

📈 Test 4: Querying Balances...
   Found 5 recent balances
   - 2024-10-18 | 5432.10 | Account: Savings
   ...

📊 Test 5: Database Statistics...
   Total Banks: 10
   Total Accounts: 25
   Total Transactions: 1500
   Total Balances: 300
   Total Cards: 5

✅ All tests completed successfully!

🎉 Finance Domain is working correctly with your existing database!
```

## What This Tests

### 1. Bank Entity
- Maps to: `MM_BankMasterData`
- Navigation: Bank → Accounts

### 2. Account Entity
- Maps to: `MM_AccountMasterData`
- Column mapping: `AccountNumber` → `Conto`
- Navigation: Account → Bank, Account → Transactions, Account → Balances

### 3. Transaction Entity
- Maps to: `TX_Transaction`
- Column mappings:
  - `TransactionDate` → `TxnDate`
  - `Amount` → `TxnAmount`
- **TransactionCategory Value Object:**
  - `Category.Name` → `Area` column
  - `Category.IsConfirmed` → `IsCatConfirmed` column
- Navigation: Transaction → Account

### 4. Balance Entity
- Maps to: `MM_Balance`
- Column mappings:
  - `Amount` → `BalanceValue`
  - `BalanceDate` → `DateBalance`
- Navigation: Balance → Account

### 5. Card Entity
- Maps to: `MM_CardMasterData`
- Navigation: Card → Account

## Troubleshooting

### Database not found
```
⚠️  Database not found at: /data/MM.db
```
**Solution:** Update `dbPath` variable in Program.cs

### Table does not exist
```
❌ Error: SQLite Error 1: 'no such table: TX_Transaction'
```
**Solution:** Check if your database has the expected tables:
```bash
sqlite3 /path/to/MM.db ".tables"
```

### Complex type error
```
❌ Error: The entity type 'TransactionCategory' requires a primary key to be defined
```
**Solution:** Ensure you're using EF Core 8.0+ which supports complex types

## Success Criteria

If you see:
```
✅ All tests completed successfully!
🎉 Finance Domain is working correctly with your existing database!
```

Then:
- ✅ Domain entities are correctly configured
- ✅ EF Core mappings to old schema work
- ✅ TransactionCategory Value Object loads correctly
- ✅ Navigation properties work
- ✅ Ready to proceed with building the API layer!
