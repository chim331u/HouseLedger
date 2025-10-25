using AutoMapper;
using FluentAssertions;
using HouseLedger.Services.Finance.Application.Contracts.Balances;
using HouseLedger.Services.Finance.Application.Mapping;
using HouseLedger.Services.Finance.Application.Services;
using HouseLedger.Services.Finance.Domain.Entities;
using HouseLedger.Services.Finance.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace HouseLedger.Services.Finance.UnitTests.Application.Services;

/// <summary>
/// Unit tests for BalanceCommandService.
/// Tests CRUD operations, audit field initialization, and error handling.
/// </summary>
public class BalanceCommandServiceTests : IDisposable
{
    private readonly FinanceDbContext _context;
    private readonly IMapper _mapper;
    private readonly BalanceCommandService _service;
    private readonly Mock<ILogger<BalanceCommandService>> _loggerMock;

    public BalanceCommandServiceTests()
    {
        // Setup in-memory database
        var options = new DbContextOptionsBuilder<FinanceDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new FinanceDbContext(options);

        // Setup AutoMapper
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<FinanceMappingProfile>();
        });
        _mapper = config.CreateMapper();

        // Setup logger mock
        _loggerMock = new Mock<ILogger<BalanceCommandService>>();

        // Create service
        _service = new BalanceCommandService(_context, _mapper, _loggerMock.Object);
    }

    #region CreateAsync Tests

    [Fact]
    public async Task CreateAsync_ValidRequest_ReturnsDto()
    {
        // Arrange
        var request = new CreateBalanceRequest
        {
            Amount = 1500.50,
            BalanceDate = DateTime.UtcNow,
            AccountId = 1,
            Note = "Test balance"
        };

        // Act
        var result = await _service.CreateAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Amount.Should().Be(request.Amount);
        result.BalanceDate.Should().BeCloseTo(request.BalanceDate, TimeSpan.FromSeconds(1));
        result.Id.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task CreateAsync_ValidRequest_SetsAuditFields()
    {
        // Arrange
        var request = new CreateBalanceRequest
        {
            Amount = 2000.00,
            BalanceDate = DateTime.UtcNow,
            AccountId = 1
        };
        var beforeCreate = DateTime.UtcNow;

        // Act
        var result = await _service.CreateAsync(request);

        // Assert
        var entity = await _context.Balances.FindAsync(result.Id);
        entity.Should().NotBeNull();
        entity!.CreatedDate.Should().BeCloseTo(beforeCreate, TimeSpan.FromSeconds(5));
        entity.LastUpdatedDate.Should().BeCloseTo(beforeCreate, TimeSpan.FromSeconds(5));
        entity.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task CreateAsync_ValidRequest_SavesToDatabase()
    {
        // Arrange
        var request = new CreateBalanceRequest
        {
            Amount = 3000.75,
            BalanceDate = DateTime.UtcNow.AddDays(-1),
            AccountId = 2
        };

        // Act
        var result = await _service.CreateAsync(request);

        // Assert
        var entityInDb = await _context.Balances.FindAsync(result.Id);
        entityInDb.Should().NotBeNull();
        entityInDb!.Amount.Should().Be(request.Amount);
        entityInDb.AccountId.Should().Be(request.AccountId);
    }

    #endregion

    #region UpdateAsync Tests

    [Fact]
    public async Task UpdateAsync_ExistingEntity_ReturnsUpdatedDto()
    {
        // Arrange
        var balance = new Balance
        {
            Amount = 1000.00,
            BalanceDate = DateTime.UtcNow.AddDays(-10),
            AccountId = 1,
            CreatedDate = DateTime.UtcNow,
            LastUpdatedDate = DateTime.UtcNow,
            IsActive = true
        };
        _context.Balances.Add(balance);
        await _context.SaveChangesAsync();

        var updateRequest = new UpdateBalanceRequest
        {
            Amount = 1500.00,
            BalanceDate = DateTime.UtcNow.AddDays(-5),
            AccountId = 2,
            Note = "Updated balance"
        };

        // Act
        var result = await _service.UpdateAsync(balance.Id, updateRequest);

        // Assert
        result.Should().NotBeNull();
        result!.Amount.Should().Be(1500.00);
        result.AccountId.Should().Be(2);
    }

    [Fact]
    public async Task UpdateAsync_NonExistentEntity_ReturnsNull()
    {
        // Arrange
        var updateRequest = new UpdateBalanceRequest
        {
            Amount = 1500.00,
            BalanceDate = DateTime.UtcNow
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
        var balance = new Balance
        {
            Amount = 500.00,
            BalanceDate = DateTime.UtcNow,
            CreatedDate = DateTime.UtcNow,
            LastUpdatedDate = DateTime.UtcNow,
            IsActive = true
        };
        _context.Balances.Add(balance);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.SoftDeleteAsync(balance.Id);

        // Assert
        result.Should().BeTrue();
        var deleted = await _context.Balances.FindAsync(balance.Id);
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
        var balance = new Balance
        {
            Amount = 750.00,
            BalanceDate = DateTime.UtcNow,
            CreatedDate = DateTime.UtcNow,
            LastUpdatedDate = DateTime.UtcNow,
            IsActive = true
        };
        _context.Balances.Add(balance);
        await _context.SaveChangesAsync();
        var createdId = balance.Id;

        // Act
        var result = await _service.HardDeleteAsync(createdId);

        // Assert
        result.Should().BeTrue();
        var exists = await _context.Balances.AnyAsync(b => b.Id == createdId);
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
