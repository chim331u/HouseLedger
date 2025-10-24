using AutoMapper;
using HouseLedger.Services.Ancillary.Application.Mapping;
using HouseLedger.Services.Ancillary.Application.Services;
using HouseLedger.Services.Ancillary.Infrastructure.Persistence;
using HouseLedger.Services.Ancillary.UnitTests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HouseLedger.Services.Ancillary.UnitTests.Application.Services;

/// <summary>
/// Unit tests for ServiceUserQueryService.
/// Tests query operations and filtering logic.
/// </summary>
public class ServiceUserQueryServiceTests : IDisposable
{
    private readonly AncillaryDbContext _context;
    private readonly IMapper _mapper;
    private readonly ServiceUserQueryService _service;
    private readonly Mock<ILogger<ServiceUserQueryService>> _loggerMock;

    public ServiceUserQueryServiceTests()
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
        _loggerMock = new Mock<ILogger<ServiceUserQueryService>>();

        // Create service
        _service = new ServiceUserQueryService(_context, _mapper, _loggerMock.Object);
    }

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_ExistingActiveEntity_ReturnsDto()
    {
        // Arrange
        var serviceUser = TestDataBuilder.ServiceUser(isActive: true);
        _context.ServiceUsers.Add(serviceUser);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetByIdAsync(serviceUser.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(serviceUser.Id);
        result.Name.Should().Be(serviceUser.Name);
        result.Surname.Should().Be(serviceUser.Surname);
    }

    [Fact]
    public async Task GetByIdAsync_InactiveEntity_ReturnsNull()
    {
        // Arrange
        var serviceUser = TestDataBuilder.ServiceUser(isActive: true);
        _context.ServiceUsers.Add(serviceUser);
        await _context.SaveChangesAsync();

        // Now set to inactive after initial save
        serviceUser.IsActive = false;
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetByIdAsync(serviceUser.Id);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_NonExistentEntity_ReturnsNull()
    {
        // Arrange
        var nonExistentId = 9999;

        // Act
        var result = await _service.GetByIdAsync(nonExistentId);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetAllAsync Tests

    [Fact]
    public async Task GetAllAsync_WithActiveEntities_ReturnsOnlyActive()
    {
        // Arrange
        var activeUser1 = TestDataBuilder.ServiceUser(name: "John", surname: "Doe", isActive: true);
        var activeUser2 = TestDataBuilder.ServiceUser(name: "Jane", surname: "Smith", isActive: true);
        var inactiveUser = TestDataBuilder.ServiceUser(name: "Bob", surname: "Inactive", isActive: true);

        _context.ServiceUsers.AddRange(activeUser1, activeUser2, inactiveUser);
        await _context.SaveChangesAsync();

        // Set one to inactive after initial save
        inactiveUser.IsActive = false;
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetAllAsync(includeInactive: false);

        // Assert
        result.Should().HaveCount(2);
        result.Should().NotContain(su => su.Name == "Bob");
    }

    [Fact]
    public async Task GetAllAsync_IncludeInactive_ReturnsAll()
    {
        // Arrange
        var activeUser = TestDataBuilder.ServiceUser(isActive: true);
        var inactiveUser = TestDataBuilder.ServiceUser(isActive: true);

        _context.ServiceUsers.AddRange(activeUser, inactiveUser);
        await _context.SaveChangesAsync();

        // Set one to inactive after initial save
        inactiveUser.IsActive = false;
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetAllAsync(includeInactive: true);

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAllAsync_OrderedBySurnameAndName()
    {
        // Arrange
        var user1 = TestDataBuilder.ServiceUser(name: "Charlie", surname: "Zulu", isActive: true);
        var user2 = TestDataBuilder.ServiceUser(name: "Alice", surname: "Alpha", isActive: true);
        var user3 = TestDataBuilder.ServiceUser(name: "Bob", surname: "Alpha", isActive: true);

        _context.ServiceUsers.AddRange(user1, user2, user3);
        await _context.SaveChangesAsync();

        // Act
        var result = (await _service.GetAllAsync(includeInactive: false)).ToList();

        // Assert
        result.Should().HaveCount(3);
        result[0].Surname.Should().Be("Alpha");
        result[0].Name.Should().Be("Alice");
        result[1].Surname.Should().Be("Alpha");
        result[1].Name.Should().Be("Bob");
        result[2].Surname.Should().Be("Zulu");
        result[2].Name.Should().Be("Charlie");
    }

    [Fact]
    public async Task GetAllAsync_EmptyDatabase_ReturnsEmptyList()
    {
        // Act
        var result = await _service.GetAllAsync();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllAsync_OnlyInactiveEntities_ReturnsEmpty()
    {
        // Arrange
        var inactiveUser1 = TestDataBuilder.ServiceUser(isActive: true);
        var inactiveUser2 = TestDataBuilder.ServiceUser(isActive: true);

        _context.ServiceUsers.AddRange(inactiveUser1, inactiveUser2);
        await _context.SaveChangesAsync();

        // Set both to inactive after initial save
        inactiveUser1.IsActive = false;
        inactiveUser2.IsActive = false;
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetAllAsync(includeInactive: false);

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
