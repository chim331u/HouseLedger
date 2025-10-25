using AutoMapper;
using FluentAssertions;
using HouseLedger.Services.HouseThings.Application.Contracts.HouseThings;
using HouseLedger.Services.HouseThings.Application.Mapping;
using HouseLedger.Services.HouseThings.Application.Services;
using HouseLedger.Services.HouseThings.Domain.Entities;
using HouseLedger.Services.HouseThings.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace HouseLedger.Services.HouseThings.UnitTests.Application.Services;

/// <summary>
/// Unit tests for HouseThingCommandService.
/// Tests CRUD operations, audit field initialization, renew functionality, and error handling.
/// </summary>
public class HouseThingCommandServiceTests : IDisposable
{
    private readonly HouseThingsDbContext _context;
    private readonly IMapper _mapper;
    private readonly HouseThingCommandService _service;
    private readonly Mock<ILogger<HouseThingCommandService>> _loggerMock;

    public HouseThingCommandServiceTests()
    {
        // Setup in-memory database
        var options = new DbContextOptionsBuilder<HouseThingsDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new HouseThingsDbContext(options);

        // Setup AutoMapper
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<HouseThingsMappingProfile>();
        });
        _mapper = config.CreateMapper();

        // Setup logger mock
        _loggerMock = new Mock<ILogger<HouseThingCommandService>>();

        // Create service
        _service = new HouseThingCommandService(_context, _mapper, _loggerMock.Object);
    }

    #region CreateAsync Tests

    [Fact]
    public async Task CreateAsync_ValidRequest_ReturnsDto()
    {
        // Arrange
        var request = new CreateHouseThingRequest
        {
            Name = "Refrigerator",
            Description = "Samsung French Door",
            ItemType = "Appliance",
            Model = "RF28R7351SR",
            Cost = 1599.99,
            HistoryId = 1,
            PurchaseDate = DateTime.UtcNow.AddDays(-30),
            RoomId = 1,
            Note = "5 year warranty"
        };

        // Act
        var result = await _service.CreateAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be(request.Name);
        result.Description.Should().Be(request.Description);
        result.ItemType.Should().Be(request.ItemType);
        result.Model.Should().Be(request.Model);
        result.Cost.Should().Be(request.Cost);
        result.HistoryId.Should().Be(request.HistoryId);
        result.Id.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task CreateAsync_ValidRequest_SetsAuditFields()
    {
        // Arrange
        var request = new CreateHouseThingRequest
        {
            Name = "Microwave",
            Description = "Panasonic Microwave",
            Cost = 299.99,
            HistoryId = 2,
            PurchaseDate = DateTime.UtcNow
        };
        var beforeCreate = DateTime.UtcNow;

        // Act
        var result = await _service.CreateAsync(request);

        // Assert
        var entity = await _context.HouseThings.FindAsync(result.Id);
        entity.Should().NotBeNull();
        entity!.CreatedDate.Should().BeCloseTo(beforeCreate, TimeSpan.FromSeconds(5));
        entity.LastUpdatedDate.Should().BeCloseTo(beforeCreate, TimeSpan.FromSeconds(5));
        entity.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task CreateAsync_ValidRequest_SavesToDatabase()
    {
        // Arrange
        var request = new CreateHouseThingRequest
        {
            Name = "Dishwasher",
            Description = "Bosch Dishwasher",
            Cost = 899.99,
            HistoryId = 3,
            PurchaseDate = DateTime.UtcNow
        };

        // Act
        var result = await _service.CreateAsync(request);

        // Assert
        var entityInDb = await _context.HouseThings.FindAsync(result.Id);
        entityInDb.Should().NotBeNull();
        entityInDb!.Name.Should().Be(request.Name);
        entityInDb.Description.Should().Be(request.Description);
        entityInDb.Cost.Should().Be(request.Cost);
    }

    #endregion

    #region UpdateAsync Tests

    [Fact]
    public async Task UpdateAsync_ExistingEntity_ReturnsUpdatedDto()
    {
        // Arrange
        var houseThing = new HouseThing
        {
            Name = "Original Thing",
            Description = "Original description",
            Cost = 100.00,
            HistoryId = 1,
            PurchaseDate = DateTime.UtcNow,
            CreatedDate = DateTime.UtcNow,
            LastUpdatedDate = DateTime.UtcNow,
            IsActive = true
        };
        _context.HouseThings.Add(houseThing);
        await _context.SaveChangesAsync();

        var updateRequest = new UpdateHouseThingRequest
        {
            Name = "Updated Thing",
            Description = "Updated description",
            ItemType = "Electronics",
            Model = "XYZ-123",
            Cost = 150.00
        };

        // Act
        var result = await _service.UpdateAsync(houseThing.Id, updateRequest);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Updated Thing");
        result.Description.Should().Be("Updated description");
        result.ItemType.Should().Be("Electronics");
        result.Model.Should().Be("XYZ-123");
        result.Cost.Should().Be(150.00);
    }

    [Fact]
    public async Task UpdateAsync_NonExistentEntity_ReturnsNull()
    {
        // Arrange
        var updateRequest = new UpdateHouseThingRequest
        {
            Name = "Updated Thing"
        };
        var nonExistentId = 9999;

        // Act
        var result = await _service.UpdateAsync(nonExistentId, updateRequest);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region RenewAsync Tests

    [Fact]
    public async Task RenewAsync_ExistingEntity_CreatesNewVersionWithSameHistoryId()
    {
        // Arrange
        var originalHistoryId = 100;
        var oldHouseThing = new HouseThing
        {
            Name = "Old Refrigerator",
            Description = "Old model",
            Cost = 1000.00,
            HistoryId = originalHistoryId,
            PurchaseDate = DateTime.UtcNow.AddYears(-5),
            CreatedDate = DateTime.UtcNow.AddYears(-5),
            LastUpdatedDate = DateTime.UtcNow.AddYears(-5),
            IsActive = true
        };
        _context.HouseThings.Add(oldHouseThing);
        await _context.SaveChangesAsync();
        var oldId = oldHouseThing.Id;

        var renewRequest = new CreateHouseThingRequest
        {
            Name = "New Refrigerator",
            Description = "New model",
            Cost = 1500.00,
            HistoryId = 999, // This should be overridden with originalHistoryId
            PurchaseDate = DateTime.UtcNow
        };

        // Act
        var result = await _service.RenewAsync(oldId, renewRequest);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("New Refrigerator");
        result.HistoryId.Should().Be(originalHistoryId); // Should match original
        result.Id.Should().NotBe(oldId); // Should be a new entity

        // Verify old entity is soft deleted
        var oldEntity = await _context.HouseThings.FindAsync(oldId);
        oldEntity.Should().NotBeNull();
        oldEntity!.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task RenewAsync_ExistingEntity_SoftDeletesOldEntity()
    {
        // Arrange
        var oldHouseThing = new HouseThing
        {
            Name = "Old Item",
            Cost = 100.00,
            HistoryId = 200,
            PurchaseDate = DateTime.UtcNow.AddYears(-1),
            CreatedDate = DateTime.UtcNow.AddYears(-1),
            LastUpdatedDate = DateTime.UtcNow.AddYears(-1),
            IsActive = true
        };
        _context.HouseThings.Add(oldHouseThing);
        await _context.SaveChangesAsync();
        var oldId = oldHouseThing.Id;

        var renewRequest = new CreateHouseThingRequest
        {
            Name = "New Item",
            Cost = 150.00,
            HistoryId = 1,
            PurchaseDate = DateTime.UtcNow
        };

        // Act
        await _service.RenewAsync(oldId, renewRequest);

        // Assert
        var oldEntity = await _context.HouseThings.FindAsync(oldId);
        oldEntity.Should().NotBeNull();
        oldEntity!.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task RenewAsync_NonExistentEntity_ThrowsInvalidOperationException()
    {
        // Arrange
        var nonExistentId = 9999;
        var renewRequest = new CreateHouseThingRequest
        {
            Name = "New Item",
            Cost = 100.00,
            HistoryId = 1,
            PurchaseDate = DateTime.UtcNow
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _service.RenewAsync(nonExistentId, renewRequest));
    }

    #endregion

    #region SoftDeleteAsync Tests

    [Fact]
    public async Task SoftDeleteAsync_ExistingEntity_SetsIsActiveFalse()
    {
        // Arrange
        var houseThing = new HouseThing
        {
            Name = "Test Thing",
            Cost = 50.00,
            HistoryId = 1,
            PurchaseDate = DateTime.UtcNow,
            CreatedDate = DateTime.UtcNow,
            LastUpdatedDate = DateTime.UtcNow,
            IsActive = true
        };
        _context.HouseThings.Add(houseThing);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.SoftDeleteAsync(houseThing.Id);

        // Assert
        result.Should().BeTrue();
        var deleted = await _context.HouseThings.FindAsync(houseThing.Id);
        deleted.Should().NotBeNull();
        deleted!.IsActive.Should().BeFalse();
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
        var houseThing = new HouseThing
        {
            Name = "Test Thing",
            Cost = 50.00,
            HistoryId = 1,
            PurchaseDate = DateTime.UtcNow,
            CreatedDate = DateTime.UtcNow,
            LastUpdatedDate = DateTime.UtcNow,
            IsActive = true
        };
        _context.HouseThings.Add(houseThing);
        await _context.SaveChangesAsync();
        var createdId = houseThing.Id;

        // Act
        var result = await _service.HardDeleteAsync(createdId);

        // Assert
        result.Should().BeTrue();
        var exists = await _context.HouseThings.AnyAsync(h => h.Id == createdId);
        exists.Should().BeFalse();
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

    #endregion

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
