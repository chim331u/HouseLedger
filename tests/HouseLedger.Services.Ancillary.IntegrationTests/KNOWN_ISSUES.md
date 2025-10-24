# Known Issues with Integration Tests

## SQLite Limitations

The integration tests use SQLite in-memory database which has some limitations compared to production databases:

### 1. String Length Constraints
SQLite does **NOT** enforce `HasMaxLength()` constraints. You can store strings longer than the specified maximum.

**Affected Tests:**
- `CountryConfiguration_MaxLengthConstraints_Enforced`
- Similar tests for other entities

**Solution:** These tests should be marked with `[Fact(Skip = "SQLite does not enforce max length constraints")]` or removed.

### 2. NULL Constraints with Default Values
When entity properties have default values (e.g., `= string.Empty`), setting them to `null!` in tests may not work as expected.

**Example:**
```csharp
var country = new Country
{
    Name = null!, // Won't actually be null due to default value
    CountryCodeAlf3 = "TST"
};
```

**Solution:** Tests expecting NULL constraint violations should create entities without setting the property at all, or use reflection to bypass defaults.

### 3. Test Isolation
Each test should get a fresh database to avoid cross-contamination.

**Current Implementation:** ✓ Correctly implemented via `IntegrationTestBase` with unique connection per test

## Running Tests

To run integration tests:
```bash
dotnet test tests/HouseLedger.Services.Ancillary.IntegrationTests
```

To run unit tests:
```bash
dotnet test tests/HouseLedger.Services.Ancillary.UnitTests
```

## Troubleshooting

If tests fail:
1. Check that all referenced projects build successfully
2. Ensure .NET 8.0 SDK is installed
3. Clear build artifacts: `dotnet clean`
4. Rebuild: `dotnet build`
5. Run tests with verbose output: `dotnet test --verbosity detailed`
