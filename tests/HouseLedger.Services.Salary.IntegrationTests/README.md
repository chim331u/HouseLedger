# HouseLedger.Services.Salary.IntegrationTests

Integration tests for the Salary service.

## Purpose

Integration tests verify that all components work together correctly:
- Database operations with SQLite
- Service layer integration
- DbContext and EF Core configurations
- End-to-end workflows

## Running Tests

```bash
# Run all integration tests
dotnet test tests/HouseLedger.Services.Salary.IntegrationTests/HouseLedger.Services.Salary.IntegrationTests.csproj

# Run with detailed output
dotnet test tests/HouseLedger.Services.Salary.IntegrationTests/HouseLedger.Services.Salary.IntegrationTests.csproj --logger "console;verbosity=detailed"
```

## Test Categories

### Database Integration Tests
- Verify entity configurations
- Test database migrations
- Validate relationships and constraints

### Service Integration Tests
- Test complete workflows
- Verify transaction boundaries
- Test concurrent operations

## Configuration

Integration tests use:
- SQLite in-memory database for isolation
- Actual DbContext configurations
- Real service implementations (not mocked)

## Best Practices

1. **Isolation**: Each test creates a fresh database
2. **Cleanup**: Tests dispose of DbContext after completion
3. **Real Data**: Use realistic test data
4. **Performance**: Keep integration tests fast
