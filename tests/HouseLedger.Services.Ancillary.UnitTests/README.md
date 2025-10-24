# HouseLedger.Services.Ancillary.UnitTests

Unit tests for the Ancillary service (Country, Currency, CurrencyConversionRate, Supplier).

## Setup

The test project includes:
- **xUnit** - Testing framework
- **Moq** - Mocking library
- **FluentAssertions** - Fluent assertion library
- **Microsoft.EntityFrameworkCore.InMemory** - In-memory database for testing
- **Bogus** - Fake data generation

## Project Structure

```
HouseLedger.Services.Ancillary.UnitTests/
├── Application/
│   └── Services/
│       └── CountryCommandServiceTests.cs   # Command service tests
├── Fixtures/
│   └── TestDataBuilder.cs                  # Test data builder using Bogus
├── HouseLedger.Services.Ancillary.UnitTests.csproj
└── README.md
```

## Running Tests

### Build the project
```bash
dotnet build tests/HouseLedger.Services.Ancillary.UnitTests/HouseLedger.Services.Ancillary.UnitTests.csproj
```

### Run all tests
```bash
dotnet test tests/HouseLedger.Services.Ancillary.UnitTests/HouseLedger.Services.Ancillary.UnitTests.csproj
```

### Run tests with detailed output
```bash
dotnet test tests/HouseLedger.Services.Ancillary.UnitTests/HouseLedger.Services.Ancillary.UnitTests.csproj --logger "console;verbosity=detailed"
```

### Run tests with coverage
```bash
dotnet test tests/HouseLedger.Services.Ancillary.UnitTests/HouseLedger.Services.Ancillary.UnitTests.csproj --collect:"XPlat Code Coverage"
```

## Test Coverage

Current implementation includes tests for:

### CountryCommandService (17 tests)
- ✅ CreateAsync_ValidRequest_ReturnsDto
- ✅ CreateAsync_ValidRequest_SetsAuditFields
- ✅ CreateAsync_ValidRequest_SavesToDatabase
- ✅ CreateAsync_MultipleCountries_AllSaved
- ✅ UpdateAsync_ExistingEntity_ReturnsUpdatedDto
- ✅ UpdateAsync_ExistingEntity_UpdatesLastUpdatedDate
- ✅ UpdateAsync_NonExistentEntity_ReturnsNull
- ✅ UpdateAsync_ExistingEntity_DoesNotChangeCreatedDate
- ✅ SoftDeleteAsync_ExistingEntity_SetsIsActiveFalse
- ✅ SoftDeleteAsync_ExistingEntity_DoesNotRemoveFromDatabase
- ✅ SoftDeleteAsync_NonExistentEntity_ReturnsFalse
- ✅ HardDeleteAsync_ExistingEntity_RemovesFromDatabase
- ✅ HardDeleteAsync_NonExistentEntity_ReturnsFalse
- ✅ HardDeleteAsync_ExistingEntity_DecreasesCount

## Next Steps

To complete the test suite:

1. **Add Query Service Tests** - CountryQueryService
2. **Add Other Entity Tests** - Currency, CurrencyConversionRate, Supplier
3. **Add AutoMapper Tests** - Verify mapping configurations
4. **Add Integration Tests** - DbContext and end-to-end tests

## Test Patterns

### Testing Services with In-Memory Database

```csharp
public class MyServiceTests : IDisposable
{
    private readonly AncillaryDbContext _context;
    private readonly IMapper _mapper;
    private readonly MyService _service;

    public MyServiceTests()
    {
        // Setup in-memory database
        var options = new DbContextOptionsBuilder<AncillaryDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AncillaryDbContext(options);

        // Setup other dependencies...
    }

    [Fact]
    public async Task MyTest()
    {
        // Arrange
        var entity = TestDataBuilder.MyEntity();

        // Act
        var result = await _service.DoSomething(entity);

        // Assert
        result.Should().NotBeNull();
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
```

### Using TestDataBuilder

```csharp
// Create with default values
var country = TestDataBuilder.Country();

// Create with specific values
var country = TestDataBuilder.Country(
    name: "United States",
    countryCodeAlf3: "USA",
    isActive: true
);

// Create request DTOs
var request = TestDataBuilder.CreateCountryRequest();
```

## Code Coverage Targets

- **Unit Tests**: 80% minimum coverage
- **Critical Paths**: 100% coverage
- **Overall Project**: 75% target

## Contributing

When adding new tests:
1. Follow the existing test naming convention: `MethodName_Scenario_ExpectedResult`
2. Use TestDataBuilder for creating test data
3. Use FluentAssertions for assertions
4. Keep tests focused and independent
5. Clean up resources in Dispose method
