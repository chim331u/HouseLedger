# HouseLedger.Services.Ancillary.IntegrationTests

Integration tests for the Ancillary Service using real SQLite database.

## Overview

This project contains comprehensive integration tests for the Ancillary Service, focusing on:
- Real database operations with SQLite in-memory
- Entity Framework Core configurations and constraints
- Audit field behavior (CreatedDate, LastUpdatedDate, IsActive)
- Full CRUD lifecycle for all entities
- Transaction handling and database constraints

## Test Structure

### Infrastructure Tests
- **IntegrationTestBase.cs** - Base class providing SQLite in-memory database
- **AncillaryDbContextTests.cs** - Tests for DbContext SaveChangesAsync behavior
- **EntityConfigurationTests.cs** - Tests for entity configurations and constraints

### Service Integration Tests
- **CountryCommandServiceIntegrationTests.cs** - Country CRUD operations
- **CurrencyCommandServiceIntegrationTests.cs** - Currency CRUD operations
- **CurrencyConversionRateCommandServiceIntegrationTests.cs** - Conversion rate operations
- **SupplierCommandServiceIntegrationTests.cs** - Supplier CRUD operations

### Test Fixtures
- **TestDataBuilder.cs** - Fake data generation using Bogus library

## Running Tests

```bash
# Run all integration tests
dotnet test

# Run with detailed output
dotnet test --logger "console;verbosity=detailed"

# Run specific test class
dotnet test --filter "FullyQualifiedName~CountryCommandServiceIntegrationTests"
```

## Test Coverage

| Component | Tests | Focus Areas |
|-----------|-------|-------------|
| DbContext | 7 | Audit fields, batch operations, updates |
| Entity Configuration | 15 | Constraints, validations, precision |
| Country Service | 20 | CRUD, persistence, concurrency |
| Currency Service | 7 | CRUD, multiple currencies |
| Conversion Rate Service | 11 | Decimals, dates, uniqueness |
| Supplier Service | 16 | CRUD, types, partial updates |
| **Total** | **76** | **Complete integration coverage** |

## Dependencies

- xUnit 2.9.2 - Test framework
- FluentAssertions 8.7.1 - Assertion library
- Moq 4.20.72 - Mocking framework
- Bogus 35.6.4 - Fake data generation
- Microsoft.EntityFrameworkCore.Sqlite 8.0.11 - SQLite provider
- Microsoft.NET.Test.Sdk 17.11.1 - Test SDK

## Key Features

### Real Database Testing
- Uses SQLite in-memory database (not EF's in-memory provider)
- Tests actual SQL constraints and database behavior
- Verifies migrations and schema configurations

### Test Isolation
- Each test gets a fresh database instance
- Connection opened at test start, closed at disposal
- No test pollution or cross-test dependencies

### Comprehensive Assertions
- Change tracker clearing for true persistence verification
- Time-based assertions with tolerance (5 seconds)
- End-to-end workflow validation
- Database constraint violation testing

## Notes

- All tests use the AAA (Arrange-Act-Assert) pattern
- Context.ChangeTracker.Clear() is used to ensure fresh reads from database
- Audit fields are tested both explicitly and implicitly
- Soft delete vs hard delete scenarios are covered
