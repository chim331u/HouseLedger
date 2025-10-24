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
/// Unit tests for ServiceUserCommandService.
/// Tests CRUD operations, audit field initialization, and error handling.
/// </summary>
public class ServiceUserCommandServiceTests : IDisposable
{
    private readonly AncillaryDbContext _context;
    private readonly IMapper _mapper;
    private readonly ServiceUserCommandService _service;
    private readonly Mock<ILogger<ServiceUserCommandService>> _loggerMock;

    public ServiceUserCommandServiceTests()
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
        _loggerMock = new Mock<ILogger<ServiceUserCommandService>>();

        // Create service
        _service = new ServiceUserCommandService(_context, _mapper, _loggerMock.Object);
    }

    #region CreateAsync Tests

    [Fact]
    public async Task CreateAsync_ValidRequest_ReturnsDto()
    {
        // Arrange
        var request = TestDataBuilder.CreateServiceUserRequest();

        // Act
        var result = await _service.CreateAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be(request.Name);
        result.Surname.Should().Be(request.Surname);
        result.Note.Should().Be(request.Note);
        result.Id.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task CreateAsync_ValidRequest_SetsAuditFields()
    {
        // Arrange
        var request = TestDataBuilder.CreateServiceUserRequest();
        var beforeCreate = DateTime.UtcNow;

        // Act
        var result = await _service.CreateAsync(request);

        // Assert
        var entity = await _context.ServiceUsers.FindAsync(result.Id);
        entity.Should().NotBeNull();
        entity!.CreatedDate.Should().BeCloseTo(beforeCreate, TimeSpan.FromSeconds(5));
        entity.LastUpdatedDate.Should().BeCloseTo(beforeCreate, TimeSpan.FromSeconds(5));
        entity.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task CreateAsync_ValidRequest_SavesToDatabase()
    {
        // Arrange
        var request = TestDataBuilder.CreateServiceUserRequest();

        // Act
        var result = await _service.CreateAsync(request);

        // Assert
        var entityInDb = await _context.ServiceUsers.FindAsync(result.Id);
        entityInDb.Should().NotBeNull();
        entityInDb!.Name.Should().Be(request.Name);
        entityInDb.Surname.Should().Be(request.Surname);
    }

    [Fact]
    public async Task CreateAsync_MultipleServiceUsers_AllSaved()
    {
        // Arrange
        var request1 = TestDataBuilder.CreateServiceUserRequest(name: "John", surname: "Doe");
        var request2 = TestDataBuilder.CreateServiceUserRequest(name: "Jane", surname: "Smith");

        // Act
        var result1 = await _service.CreateAsync(request1);
        var result2 = await _service.CreateAsync(request2);

        // Assert
        var serviceUsers = await _context.ServiceUsers.ToListAsync();
        serviceUsers.Should().HaveCount(2);
        serviceUsers.Should().Contain(su => su.Name == "John" && su.Surname == "Doe");
        serviceUsers.Should().Contain(su => su.Name == "Jane" && su.Surname == "Smith");
    }

    #endregion

    #region UpdateAsync Tests

    [Fact]
    public async Task UpdateAsync_ExistingEntity_ReturnsUpdatedDto()
    {
        // Arrange
        var serviceUser = TestDataBuilder.ServiceUser();
        _context.ServiceUsers.Add(serviceUser);
        await _context.SaveChangesAsync();

        var updateRequest = TestDataBuilder.UpdateServiceUserRequest(name: "UpdatedName", surname: "UpdatedSurname");

        // Act
        var result = await _service.UpdateAsync(serviceUser.Id, updateRequest);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("UpdatedName");
        result.Surname.Should().Be("UpdatedSurname");
        result.Id.Should().Be(serviceUser.Id);
    }

    [Fact]
    public async Task UpdateAsync_ExistingEntity_UpdatesLastUpdatedDate()
    {
        // Arrange
        var serviceUser = TestDataBuilder.ServiceUser();
        serviceUser.LastUpdatedDate = DateTime.UtcNow.AddDays(-1);
        _context.ServiceUsers.Add(serviceUser);
        await _context.SaveChangesAsync();

        var updateRequest = TestDataBuilder.UpdateServiceUserRequest();
        var beforeUpdate = DateTime.UtcNow;

        // Act
        await _service.UpdateAsync(serviceUser.Id, updateRequest);

        // Assert
        var updated = await _context.ServiceUsers.FindAsync(serviceUser.Id);
        updated!.LastUpdatedDate.Should().BeCloseTo(beforeUpdate, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task UpdateAsync_NonExistentEntity_ReturnsNull()
    {
        // Arrange
        var updateRequest = TestDataBuilder.UpdateServiceUserRequest();
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
        var serviceUser = TestDataBuilder.ServiceUser();
        _context.ServiceUsers.Add(serviceUser);
        await _context.SaveChangesAsync();

        // Capture the actual created date after initial save
        var originalCreatedDate = serviceUser.CreatedDate;
        var originalName = serviceUser.Name;

        var updateRequest = TestDataBuilder.UpdateServiceUserRequest(name: "NewName");

        // Act
        await _service.UpdateAsync(serviceUser.Id, updateRequest);

        // Assert
        var updated = await _context.ServiceUsers.FindAsync(serviceUser.Id);
        updated!.CreatedDate.Should().Be(originalCreatedDate);
        updated.Name.Should().Be("NewName");
        updated.Name.Should().NotBe(originalName);
    }

    #endregion

    #region SoftDeleteAsync Tests

    [Fact]
    public async Task SoftDeleteAsync_ExistingEntity_SetsIsActiveFalse()
    {
        // Arrange
        var serviceUser = TestDataBuilder.ServiceUser(isActive: true);
        _context.ServiceUsers.Add(serviceUser);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.SoftDeleteAsync(serviceUser.Id);

        // Assert
        result.Should().BeTrue();
        var deleted = await _context.ServiceUsers.FindAsync(serviceUser.Id);
        deleted.Should().NotBeNull();
        deleted!.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task SoftDeleteAsync_ExistingEntity_DoesNotRemoveFromDatabase()
    {
        // Arrange
        var serviceUser = TestDataBuilder.ServiceUser();
        _context.ServiceUsers.Add(serviceUser);
        await _context.SaveChangesAsync();

        // Act
        await _service.SoftDeleteAsync(serviceUser.Id);

        // Assert
        var entityStillExists = await _context.ServiceUsers.AnyAsync(su => su.Id == serviceUser.Id);
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
        var serviceUser = TestDataBuilder.ServiceUser();
        _context.ServiceUsers.Add(serviceUser);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.HardDeleteAsync(serviceUser.Id);

        // Assert
        result.Should().BeTrue();
        var entityExists = await _context.ServiceUsers.AnyAsync(su => su.Id == serviceUser.Id);
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
        var serviceUser1 = TestDataBuilder.ServiceUser();
        var serviceUser2 = TestDataBuilder.ServiceUser();
        _context.ServiceUsers.AddRange(serviceUser1, serviceUser2);
        await _context.SaveChangesAsync();

        var initialCount = await _context.ServiceUsers.CountAsync();

        // Act
        await _service.HardDeleteAsync(serviceUser1.Id);

        // Assert
        var finalCount = await _context.ServiceUsers.CountAsync();
        finalCount.Should().Be(initialCount - 1);
    }

    #endregion

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
