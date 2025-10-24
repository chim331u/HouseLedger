using AutoMapper;
using HouseLedger.Services.Ancillary.Application.Mapping;
using HouseLedger.Services.Ancillary.Application.Services;
using HouseLedger.Services.Ancillary.Domain.Entities;
using HouseLedger.Services.Ancillary.Infrastructure.Persistence;
using HouseLedger.Services.Ancillary.UnitTests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HouseLedger.Services.Ancillary.UnitTests.Application.Services;

/// <summary>
/// Unit tests for CountryCommandService.
/// Tests CRUD operations, audit field initialization, and error handling.
/// </summary>
public class CountryCommandServiceTests : IDisposable
{
    private readonly AncillaryDbContext _context;
    private readonly IMapper _mapper;
    private readonly CountryCommandService _service;
    private readonly Mock<ILogger<CountryCommandService>> _loggerMock;

    public CountryCommandServiceTests()
    {
        // Setup in-memory database
        var options = new DbContextOptionsBuilder<AncillaryDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AncillaryDbContext(options);

        // Setup AutoMapper
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<AncillaryMappingProfile>();
        });
        _mapper = config.CreateMapper();

        // Setup logger mock
        _loggerMock = new Mock<ILogger<CountryCommandService>>();

        // Create service
        _service = new CountryCommandService(_context, _mapper, _loggerMock.Object);
    }

    #region CreateAsync Tests

    [Fact]
    public async Task CreateAsync_ValidRequest_ReturnsDto()
    {
        // Arrange
        var request = TestDataBuilder.CreateCountryRequest();

        // Act
        var result = await _service.CreateAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be(request.Name);
        result.CountryCodeAlf3.Should().Be(request.CountryCodeAlf3);
        result.CountryCodeNum3.Should().Be(request.CountryCodeNum3);
        result.Description.Should().Be(request.Description);
        result.Id.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task CreateAsync_ValidRequest_SetsAuditFields()
    {
        // Arrange
        var request = TestDataBuilder.CreateCountryRequest();
        var beforeCreate = DateTime.UtcNow;

        // Act
        var result = await _service.CreateAsync(request);

        // Assert
        var entity = await _context.Countries.FindAsync(result.Id);
        entity.Should().NotBeNull();
        entity!.CreatedDate.Should().BeCloseTo(beforeCreate, TimeSpan.FromSeconds(5));
        entity.LastUpdatedDate.Should().BeCloseTo(beforeCreate, TimeSpan.FromSeconds(5));
        entity.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task CreateAsync_ValidRequest_SavesToDatabase()
    {
        // Arrange
        var request = TestDataBuilder.CreateCountryRequest();

        // Act
        var result = await _service.CreateAsync(request);

        // Assert
        var entityInDb = await _context.Countries.FindAsync(result.Id);
        entityInDb.Should().NotBeNull();
        entityInDb!.Name.Should().Be(request.Name);
    }

    [Fact]
    public async Task CreateAsync_MultipleCountries_AllSaved()
    {
        // Arrange
        var request1 = TestDataBuilder.CreateCountryRequest(name: "USA");
        var request2 = TestDataBuilder.CreateCountryRequest(name: "Canada");

        // Act
        var result1 = await _service.CreateAsync(request1);
        var result2 = await _service.CreateAsync(request2);

        // Assert
        var countries = await _context.Countries.ToListAsync();
        countries.Should().HaveCount(2);
        countries.Should().Contain(c => c.Name == "USA");
        countries.Should().Contain(c => c.Name == "Canada");
    }

    #endregion

    #region UpdateAsync Tests

    [Fact]
    public async Task UpdateAsync_ExistingEntity_ReturnsUpdatedDto()
    {
        // Arrange
        var country = TestDataBuilder.Country();
        _context.Countries.Add(country);
        await _context.SaveChangesAsync();

        var updateRequest = TestDataBuilder.UpdateCountryRequest(name: "Updated Country");

        // Act
        var result = await _service.UpdateAsync(country.Id, updateRequest);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Updated Country");
        result.Id.Should().Be(country.Id);
    }

    [Fact]
    public async Task UpdateAsync_ExistingEntity_UpdatesLastUpdatedDate()
    {
        // Arrange
        var country = TestDataBuilder.Country();
        country.LastUpdatedDate = DateTime.UtcNow.AddDays(-1);
        _context.Countries.Add(country);
        await _context.SaveChangesAsync();

        var updateRequest = TestDataBuilder.UpdateCountryRequest();
        var beforeUpdate = DateTime.UtcNow;

        // Act
        await _service.UpdateAsync(country.Id, updateRequest);

        // Assert
        var updated = await _context.Countries.FindAsync(country.Id);
        updated!.LastUpdatedDate.Should().BeCloseTo(beforeUpdate, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task UpdateAsync_NonExistentEntity_ReturnsNull()
    {
        // Arrange
        var updateRequest = TestDataBuilder.UpdateCountryRequest();
        var nonExistentId = 9999;

        // Act
        var result = await _service.UpdateAsync(nonExistentId, updateRequest);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateAsync_ExistingEntity_DoesNotChangeCreatedDate()
    {
        // Arrange
        var country = TestDataBuilder.Country();
        _context.Countries.Add(country);
        await _context.SaveChangesAsync();

        // Capture the actual created date after initial save
        var originalCreatedDate = country.CreatedDate;
        var originalCountryCode = country.CountryCodeAlf3;

        var updateRequest = TestDataBuilder.UpdateCountryRequest();
        updateRequest.CountryCodeAlf3 = "NEW";

        // Act
        await _service.UpdateAsync(country.Id, updateRequest);

        // Assert
        var updated = await _context.Countries.FindAsync(country.Id);
        updated!.CreatedDate.Should().Be(originalCreatedDate);
        updated.CountryCodeAlf3.Should().Be("NEW");
        updated.CountryCodeAlf3.Should().NotBe(originalCountryCode);
    }

    #endregion

    #region SoftDeleteAsync Tests

    [Fact]
    public async Task SoftDeleteAsync_ExistingEntity_SetsIsActiveFalse()
    {
        // Arrange
        var country = TestDataBuilder.Country(isActive: true);
        _context.Countries.Add(country);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.SoftDeleteAsync(country.Id);

        // Assert
        result.Should().BeTrue();
        var deleted = await _context.Countries.FindAsync(country.Id);
        deleted.Should().NotBeNull();
        deleted!.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task SoftDeleteAsync_ExistingEntity_DoesNotRemoveFromDatabase()
    {
        // Arrange
        var country = TestDataBuilder.Country();
        _context.Countries.Add(country);
        await _context.SaveChangesAsync();

        // Act
        await _service.SoftDeleteAsync(country.Id);

        // Assert
        var entityStillExists = await _context.Countries.AnyAsync(c => c.Id == country.Id);
        entityStillExists.Should().BeTrue();
    }

    [Fact]
    public async Task SoftDeleteAsync_NonExistentEntity_ReturnsFalse()
    {
        // Arrange
        var nonExistentId = 9999;

        // Act
        var result = await _service.SoftDeleteAsync(nonExistentId);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region HardDeleteAsync Tests

    [Fact]
    public async Task HardDeleteAsync_ExistingEntity_RemovesFromDatabase()
    {
        // Arrange
        var country = TestDataBuilder.Country();
        _context.Countries.Add(country);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.HardDeleteAsync(country.Id);

        // Assert
        result.Should().BeTrue();
        var entityExists = await _context.Countries.AnyAsync(c => c.Id == country.Id);
        entityExists.Should().BeFalse();
    }

    [Fact]
    public async Task HardDeleteAsync_NonExistentEntity_ReturnsFalse()
    {
        // Arrange
        var nonExistentId = 9999;

        // Act
        var result = await _service.HardDeleteAsync(nonExistentId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task HardDeleteAsync_ExistingEntity_DecreasesCount()
    {
        // Arrange
        var country1 = TestDataBuilder.Country();
        var country2 = TestDataBuilder.Country();
        _context.Countries.AddRange(country1, country2);
        await _context.SaveChangesAsync();

        var initialCount = await _context.Countries.CountAsync();

        // Act
        await _service.HardDeleteAsync(country1.Id);

        // Assert
        var finalCount = await _context.Countries.CountAsync();
        finalCount.Should().Be(initialCount - 1);
    }

    #endregion

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
