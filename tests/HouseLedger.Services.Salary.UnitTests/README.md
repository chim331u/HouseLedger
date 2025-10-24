# HouseLedger.Services.Salary.UnitTests

Unit tests for the Salary service.

## Running Tests

```bash
# Build the test project
dotnet build tests/HouseLedger.Services.Salary.UnitTests/HouseLedger.Services.Salary.UnitTests.csproj

# Run all tests
dotnet test tests/HouseLedger.Services.Salary.UnitTests/HouseLedger.Services.Salary.UnitTests.csproj

# Run with detailed output
dotnet test tests/HouseLedger.Services.Salary.UnitTests/HouseLedger.Services.Salary.UnitTests.csproj --logger "console;verbosity=detailed"

# Run with code coverage
dotnet test tests/HouseLedger.Services.Salary.UnitTests/HouseLedger.Services.Salary.UnitTests.csproj --collect:"XPlat Code Coverage"
```

## Test Coverage

Current implementation includes tests for:

### SalaryCommandService
- ✅ CreateAsync_ValidRequest_ReturnsDto
- ✅ CreateAsync_ValidRequest_SetsAuditFields
- ✅ CreateAsync_ValidRequest_CalculatesEurValue
- ✅ UpdateAsync_ExistingEntity_ReturnsUpdatedDto
- ✅ UpdateAsync_NonExistentEntity_ReturnsNull
- ✅ DeleteAsync_ExistingEntity_SetsIsActiveFalse
- ✅ DeleteAsync_NonExistentEntity_ReturnsFalse

## Next Steps

To complete the test suite:

1. **Add Query Service Tests** - SalaryQueryService
2. **Add Handler Tests** - MediatR command/query handlers
3. **Add Validator Tests** - FluentValidation validators
4. **Add AutoMapper Tests** - Verify mapping configurations
