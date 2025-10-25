using AutoMapper;
using FluentAssertions;
using HouseLedger.Services.HouseThings.Application.Contracts.Rooms;
using HouseLedger.Services.HouseThings.Application.Mapping;
using HouseLedger.Services.HouseThings.Application.Services;
using HouseLedger.Services.HouseThings.Domain.Entities;
using HouseLedger.Services.HouseThings.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace HouseLedger.Services.HouseThings.UnitTests.Application.Services;

/// <summary>
/// Unit tests for RoomCommandService.
/// Tests CRUD operations, audit field initialization, and error handling.
/// </summary>
public class RoomCommandServiceTests : IDisposable
{
    private readonly HouseThingsDbContext _context;
    private readonly IMapper _mapper;
    private readonly RoomCommandService _service;
    private readonly Mock<ILogger<RoomCommandService>> _loggerMock;

    public RoomCommandServiceTests()
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
        _loggerMock = new Mock<ILogger<RoomCommandService>>();

        // Create service
        _service = new RoomCommandService(_context, _mapper, _loggerMock.Object);
    }

    #region CreateAsync Tests

    [Fact]
    public async Task CreateAsync_ValidRequest_ReturnsDto()
    {
        // Arrange
        var request = new CreateRoomRequest
        {
            Name = "Living Room",
            Description = "Main living area",
            Color = "#FF5733",
            Icon = "sofa",
            Note = "Recently renovated"
        };

        // Act
        var result = await _service.CreateAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be(request.Name);
        result.Description.Should().Be(request.Description);
        result.Color.Should().Be(request.Color);
        result.Icon.Should().Be(request.Icon);
        result.Note.Should().Be(request.Note);
        result.Id.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task CreateAsync_ValidRequest_SetsAuditFields()
    {
        // Arrange
        var request = new CreateRoomRequest
        {
            Name = "Bedroom",
            Description = "Master bedroom"
        };
        var beforeCreate = DateTime.UtcNow;

        // Act
        var result = await _service.CreateAsync(request);

        // Assert
        var entity = await _context.Rooms.FindAsync(result.Id);
        entity.Should().NotBeNull();
        entity!.CreatedDate.Should().BeCloseTo(beforeCreate, TimeSpan.FromSeconds(5));
        entity.LastUpdatedDate.Should().BeCloseTo(beforeCreate, TimeSpan.FromSeconds(5));
        entity.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task CreateAsync_ValidRequest_SavesToDatabase()
    {
        // Arrange
        var request = new CreateRoomRequest
        {
            Name = "Kitchen",
            Description = "Modern kitchen with island"
        };

        // Act
        var result = await _service.CreateAsync(request);

        // Assert
        var entityInDb = await _context.Rooms.FindAsync(result.Id);
        entityInDb.Should().NotBeNull();
        entityInDb!.Name.Should().Be(request.Name);
        entityInDb.Description.Should().Be(request.Description);
    }

    #endregion

    #region UpdateAsync Tests

    [Fact]
    public async Task UpdateAsync_ExistingEntity_ReturnsUpdatedDto()
    {
        // Arrange
        var room = new Room
        {
            Name = "Original Room",
            Description = "Original description",
            CreatedDate = DateTime.UtcNow,
            LastUpdatedDate = DateTime.UtcNow,
            IsActive = true
        };
        _context.Rooms.Add(room);
        await _context.SaveChangesAsync();

        var updateRequest = new UpdateRoomRequest
        {
            Name = "Updated Room",
            Description = "Updated description",
            Color = "#00FF00",
            Icon = "bed"
        };

        // Act
        var result = await _service.UpdateAsync(room.Id, updateRequest);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Updated Room");
        result.Description.Should().Be("Updated description");
        result.Color.Should().Be("#00FF00");
        result.Icon.Should().Be("bed");
    }

    [Fact]
    public async Task UpdateAsync_NonExistentEntity_ReturnsNull()
    {
        // Arrange
        var updateRequest = new UpdateRoomRequest
        {
            Name = "Updated Room"
        };
        var nonExistentId = 9999;

        // Act
        var result = await _service.UpdateAsync(nonExistentId, updateRequest);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region SoftDeleteAsync Tests

    [Fact]
    public async Task SoftDeleteAsync_ExistingEntity_SetsIsActiveFalse()
    {
        // Arrange
        var room = new Room
        {
            Name = "Test Room",
            CreatedDate = DateTime.UtcNow,
            LastUpdatedDate = DateTime.UtcNow,
            IsActive = true
        };
        _context.Rooms.Add(room);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.SoftDeleteAsync(room.Id);

        // Assert
        result.Should().BeTrue();
        var deleted = await _context.Rooms.FindAsync(room.Id);
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
        var room = new Room
        {
            Name = "Test Room",
            CreatedDate = DateTime.UtcNow,
            LastUpdatedDate = DateTime.UtcNow,
            IsActive = true
        };
        _context.Rooms.Add(room);
        await _context.SaveChangesAsync();
        var createdId = room.Id;

        // Act
        var result = await _service.HardDeleteAsync(createdId);

        // Assert
        result.Should().BeTrue();
        var exists = await _context.Rooms.AnyAsync(r => r.Id == createdId);
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
