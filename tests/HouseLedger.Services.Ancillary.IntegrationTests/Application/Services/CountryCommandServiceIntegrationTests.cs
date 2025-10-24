using AutoMapper;
using HouseLedger.Services.Ancillary.Application.Mapping;
using HouseLedger.Services.Ancillary.Application.Services;
using HouseLedger.Services.Ancillary.IntegrationTests.Fixtures;
using HouseLedger.Services.Ancillary.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HouseLedger.Services.Ancillary.IntegrationTests.Application.Services;

/// <summary>
/// Integration tests for CountryCommandService with real database operations.
/// Tests full CRUD lifecycle, transaction handling, and database constraints.
/// </summary>
public class CountryCommandServiceIntegrationTests : IntegrationTestBase
{
    private readonly IMapper _mapper;
    private readonly CountryCommandService _service;
    private readonly Mock<ILogger<CountryCommandService>> _loggerMock;

    public CountryCommandServiceIntegrationTests()
    {
        // Setup AutoMapper
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<AncillaryMappingProfile>();
        });
        _mapper = config.CreateMapper();

        // Setup logger mock
        _loggerMock = new Mock<ILogger<CountryCommandService>>();

        // Create service
        _service = new CountryCommandService(Context, _mapper, _loggerMock.Object);
    }

    #region CreateAsync Integration Tests

    [Fact]
    public async Task CreateAsync_ValidRequest_PersistsToDatabase()
    {
        // Arrange
        var request = TestDataBuilder.CreateCountryRequest(
            name: "Italy",
            countryCodeAlf3: "ITA",
            countryCodeNum3: "380");

        // Act
        var result = await _service.CreateAsync(request);

        // Assert - Verify DTO
        result.Should().NotBeNull();
        result.Name.Should().Be("Italy");
        result.CountryCodeAlf3.Should().Be("ITA");

        // Assert - Verify database persistence by querying fresh context
        Context.ChangeTracker.Clear();
        var persisted = await Context.Countries.FindAsync(result.Id);
        persisted.Should().NotBeNull();
        persisted!.Name.Should().Be("Italy");
        persisted.CountryCodeAlf3.Should().Be("ITA");
        persisted.CountryCodeNum3.Should().Be("380");
        persisted.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task CreateAsync_ValidRequest_SetsAuditFieldsCorrectly()
    {
        // Arrange
        var request = TestDataBuilder.CreateCountryRequest();
        var beforeCreate = DateTime.UtcNow;

        // Act
        var result = await _service.CreateAsync(request);

        // Assert
        Context.ChangeTracker.Clear();
        var entity = await Context.Countries.FindAsync(result.Id);
        entity.Should().NotBeNull();
        entity!.CreatedDate.Should().BeCloseTo(beforeCreate, TimeSpan.FromSeconds(5));
        entity.LastUpdatedDate.Should().BeCloseTo(beforeCreate, TimeSpan.FromSeconds(5));
        entity.CreatedDate.Should().Be(entity.LastUpdatedDate);
        entity.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task CreateAsync_MultipleCountries_AllPersisted()
    {
        // Arrange
        var request1 = TestDataBuilder.CreateCountryRequest(name: "France");
        var request2 = TestDataBuilder.CreateCountryRequest(name: "Germany");
        var request3 = TestDataBuilder.CreateCountryRequest(name: "Spain");

        // Act
        var result1 = await _service.CreateAsync(request1);
        var result2 = await _service.CreateAsync(request2);
        var result3 = await _service.CreateAsync(request3);

        // Assert
        var countries = await Context.Countries.ToListAsync();
        countries.Should().HaveCount(3);
        countries.Should().Contain(c => c.Name == "France");
        countries.Should().Contain(c => c.Name == "Germany");
        countries.Should().Contain(c => c.Name == "Spain");
    }

    [Fact]
    public async Task CreateAsync_WithSpecialCharacters_HandlesCorrectly()
    {
        // Arrange
        var request = TestDataBuilder.CreateCountryRequest(
            name: "Côte d'Ivoire",
            description: "Country with special chars: é, ñ, ü");

        // Act
        var result = await _service.CreateAsync(request);

        // Assert
        Context.ChangeTracker.Clear();
        var persisted = await Context.Countries.FindAsync(result.Id);
        persisted!.Name.Should().Be("Côte d'Ivoire");
        persisted.Description.Should().Contain("é, ñ, ü");
    }

    #endregion

    #region UpdateAsync Integration Tests

    [Fact]
    public async Task UpdateAsync_ExistingEntity_PersistsChanges()
    {
        // Arrange
        var createRequest = TestDataBuilder.CreateCountryRequest(name: "Original Name");
        var created = await _service.CreateAsync(createRequest);

        var updateRequest = TestDataBuilder.UpdateCountryRequest(
            name: "Updated Name",
            countryCodeAlf3: "UPD",
            note: "Updated note");

        // Act
        var result = await _service.UpdateAsync(created.Id, updateRequest);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Updated Name");

        // Verify persistence
        Context.ChangeTracker.Clear();
        var persisted = await Context.Countries.FindAsync(created.Id);
        persisted!.Name.Should().Be("Updated Name");
        persisted.CountryCodeAlf3.Should().Be("UPD");
        persisted.Note.Should().Be("Updated note");
    }

    [Fact]
    public async Task UpdateAsync_ExistingEntity_UpdatesLastUpdatedDate()
    {
        // Arrange
        var createRequest = TestDataBuilder.CreateCountryRequest();
        var created = await _service.CreateAsync(createRequest);

        Context.ChangeTracker.Clear();
        var originalEntity = await Context.Countries.FindAsync(created.Id);
        var originalCreatedDate = originalEntity!.CreatedDate;
        var originalLastUpdatedDate = originalEntity.LastUpdatedDate;

        await Task.Delay(100); // Ensure time passes
        var beforeUpdate = DateTime.UtcNow;

        var updateRequest = TestDataBuilder.UpdateCountryRequest(name: "New Name");

        // Act
        await _service.UpdateAsync(created.Id, updateRequest);

        // Assert
        Context.ChangeTracker.Clear();
        var updated = await Context.Countries.FindAsync(created.Id);
        updated!.CreatedDate.Should().Be(originalCreatedDate);
        updated.LastUpdatedDate.Should().BeAfter(originalLastUpdatedDate);
        updated.LastUpdatedDate.Should().BeCloseTo(beforeUpdate, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task UpdateAsync_NonExistentEntity_ReturnsNull()
    {
        // Arrange
        var updateRequest = TestDataBuilder.UpdateCountryRequest();
        var nonExistentId = 99999;

        // Act
        var result = await _service.UpdateAsync(nonExistentId, updateRequest);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateAsync_PartialUpdate_OnlyChangesSpecifiedFields()
    {
        // Arrange
        var createRequest = TestDataBuilder.CreateCountryRequest(
            name: "Original",
            countryCodeAlf3: "ORG",
            description: "Original description");
        var created = await _service.CreateAsync(createRequest);

        var updateRequest = TestDataBuilder.UpdateCountryRequest(
            name: "Updated Name Only",
            countryCodeAlf3: "ORG", // Keep same
            description: "Original description"); // Keep same

        // Act
        await _service.UpdateAsync(created.Id, updateRequest);

        // Assert
        Context.ChangeTracker.Clear();
        var updated = await Context.Countries.FindAsync(created.Id);
        updated!.Name.Should().Be("Updated Name Only");
        updated.CountryCodeAlf3.Should().Be("ORG");
        updated.Description.Should().Be("Original description");
    }

    #endregion

    #region SoftDeleteAsync Integration Tests

    [Fact]
    public async Task SoftDeleteAsync_ExistingEntity_SetsIsActiveFalseInDatabase()
    {
        // Arrange
        var createRequest = TestDataBuilder.CreateCountryRequest();
        var created = await _service.CreateAsync(createRequest);

        // Act
        var result = await _service.SoftDeleteAsync(created.Id);

        // Assert
        result.Should().BeTrue();

        Context.ChangeTracker.Clear();
        var deleted = await Context.Countries.FindAsync(created.Id);
        deleted.Should().NotBeNull();
        deleted!.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task SoftDeleteAsync_ExistingEntity_UpdatesLastUpdatedDate()
    {
        // Arrange
        var createRequest = TestDataBuilder.CreateCountryRequest();
        var created = await _service.CreateAsync(createRequest);

        Context.ChangeTracker.Clear();
        var original = await Context.Countries.FindAsync(created.Id);
        var originalLastUpdatedDate = original!.LastUpdatedDate;

        await Task.Delay(100);
        var beforeDelete = DateTime.UtcNow;

        // Act
        await _service.SoftDeleteAsync(created.Id);

        // Assert
        Context.ChangeTracker.Clear();
        var deleted = await Context.Countries.FindAsync(created.Id);
        deleted!.LastUpdatedDate.Should().BeAfter(originalLastUpdatedDate);
        deleted.LastUpdatedDate.Should().BeCloseTo(beforeDelete, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task SoftDeleteAsync_NonExistentEntity_ReturnsFalse()
    {
        // Arrange
        var nonExistentId = 99999;

        // Act
        var result = await _service.SoftDeleteAsync(nonExistentId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task SoftDeleteAsync_AlreadyDeleted_CanBeDeletedAgain()
    {
        // Arrange
        var createRequest = TestDataBuilder.CreateCountryRequest();
        var created = await _service.CreateAsync(createRequest);
        await _service.SoftDeleteAsync(created.Id);

        // Act
        var result = await _service.SoftDeleteAsync(created.Id);

        // Assert
        result.Should().BeTrue();

        Context.ChangeTracker.Clear();
        var deleted = await Context.Countries.FindAsync(created.Id);
        deleted!.IsActive.Should().BeFalse();
    }

    #endregion

    #region HardDeleteAsync Integration Tests

    [Fact]
    public async Task HardDeleteAsync_ExistingEntity_RemovesFromDatabase()
    {
        // Arrange
        var createRequest = TestDataBuilder.CreateCountryRequest();
        var created = await _service.CreateAsync(createRequest);
        var createdId = created.Id;

        // Act
        var result = await _service.HardDeleteAsync(createdId);

        // Assert
        result.Should().BeTrue();

        Context.ChangeTracker.Clear();
        var exists = await Context.Countries.AnyAsync(c => c.Id == createdId);
        exists.Should().BeFalse();
    }

    [Fact]
    public async Task HardDeleteAsync_NonExistentEntity_ReturnsFalse()
    {
        // Arrange
        var nonExistentId = 99999;

        // Act
        var result = await _service.HardDeleteAsync(nonExistentId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task HardDeleteAsync_AfterSoftDelete_StillRemovesFromDatabase()
    {
        // Arrange
        var createRequest = TestDataBuilder.CreateCountryRequest();
        var created = await _service.CreateAsync(createRequest);
        await _service.SoftDeleteAsync(created.Id);

        // Act
        var result = await _service.HardDeleteAsync(created.Id);

        // Assert
        result.Should().BeTrue();

        Context.ChangeTracker.Clear();
        var exists = await Context.Countries.AnyAsync(c => c.Id == created.Id);
        exists.Should().BeFalse();
    }

    #endregion

    #region Transaction and Concurrency Tests

    [Fact]
    public async Task CreateAsync_DatabaseConstraintViolation_ThrowsException()
    {
        // Arrange - Create entity directly to bypass service validation
        var country = new Domain.Entities.Country
        {
            Name = null!, // Violates NOT NULL constraint
            CountryCodeAlf3 = "TST",
            CountryCodeNum3 = "123"
        };

        // Act & Assert
        Context.Countries.Add(country);
        var act = async () => await Context.SaveChangesAsync();
        await act.Should().ThrowAsync<DbUpdateException>();
    }

    [Fact]
    public async Task FullCRUDCycle_WorksEndToEnd()
    {
        // Create
        var createRequest = TestDataBuilder.CreateCountryRequest(name: "Netherlands");
        var created = await _service.CreateAsync(createRequest);
        created.Name.Should().Be("Netherlands");

        // Read
        Context.ChangeTracker.Clear();
        var read = await Context.Countries.FindAsync(created.Id);
        read.Should().NotBeNull();
        read!.Name.Should().Be("Netherlands");

        // Update
        var updateRequest = TestDataBuilder.UpdateCountryRequest(name: "The Netherlands");
        var updated = await _service.UpdateAsync(created.Id, updateRequest);
        updated!.Name.Should().Be("The Netherlands");

        // Soft Delete
        var softDeleted = await _service.SoftDeleteAsync(created.Id);
        softDeleted.Should().BeTrue();
        Context.ChangeTracker.Clear();
        var afterSoftDelete = await Context.Countries.FindAsync(created.Id);
        afterSoftDelete!.IsActive.Should().BeFalse();

        // Hard Delete
        var hardDeleted = await _service.HardDeleteAsync(created.Id);
        hardDeleted.Should().BeTrue();
        Context.ChangeTracker.Clear();
        var afterHardDelete = await Context.Countries.FindAsync(created.Id);
        afterHardDelete.Should().BeNull();
    }

    #endregion
}
