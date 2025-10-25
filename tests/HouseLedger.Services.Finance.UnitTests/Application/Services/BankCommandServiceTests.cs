using AutoMapper;
using FluentAssertions;
using HouseLedger.Services.Finance.Application.Contracts.Banks;
using HouseLedger.Services.Finance.Application.Mapping;
using HouseLedger.Services.Finance.Application.Services;
using HouseLedger.Services.Finance.Domain.Entities;
using HouseLedger.Services.Finance.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace HouseLedger.Services.Finance.UnitTests.Application.Services;

/// <summary>
/// Unit tests for BankCommandService.
/// Tests CRUD operations, audit field initialization, and error handling.
/// </summary>
public class BankCommandServiceTests : IDisposable
{
    private readonly FinanceDbContext _context;
    private readonly IMapper _mapper;
    private readonly BankCommandService _service;
    private readonly Mock<ILogger<BankCommandService>> _loggerMock;

    public BankCommandServiceTests()
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
        _loggerMock = new Mock<ILogger<BankCommandService>>();

        // Create service
        _service = new BankCommandService(_context, _mapper, _loggerMock.Object);
    }

    #region CreateAsync Tests

    [Fact]
    public async Task CreateAsync_ValidRequest_ReturnsDto()
    {
        // Arrange
        var request = new CreateBankRequest
        {
            Name = "Test Bank",
            Description = "A test bank",
            WebUrl = "https://testbank.com",
            Address = "123 Bank Street",
            City = "Milan",
            Phone = "+39 02 1234567",
            Mail = "info@testbank.com",
            ReferenceName = "John Smith",
            CountryId = 1
        };

        // Act
        var result = await _service.CreateAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be(request.Name);
        result.Description.Should().Be(request.Description);
        result.WebUrl.Should().Be(request.WebUrl);
        result.City.Should().Be(request.City);
        result.Id.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task CreateAsync_ValidRequest_SetsAuditFields()
    {
        // Arrange
        var request = new CreateBankRequest
        {
            Name = "Test Bank",
            Description = "A test bank"
        };
        var beforeCreate = DateTime.UtcNow;

        // Act
        var result = await _service.CreateAsync(request);

        // Assert
        var entity = await _context.Banks.FindAsync(result.Id);
        entity.Should().NotBeNull();
        entity!.CreatedDate.Should().BeCloseTo(beforeCreate, TimeSpan.FromSeconds(5));
        entity.LastUpdatedDate.Should().BeCloseTo(beforeCreate, TimeSpan.FromSeconds(5));
        entity.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task CreateAsync_ValidRequest_SavesToDatabase()
    {
        // Arrange
        var request = new CreateBankRequest
        {
            Name = "Savings Bank",
            Description = "A savings bank"
        };

        // Act
        var result = await _service.CreateAsync(request);

        // Assert
        var entityInDb = await _context.Banks.FindAsync(result.Id);
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
        var bank = new Bank
        {
            Name = "Original Bank",
            Description = "Original description",
            CreatedDate = DateTime.UtcNow,
            LastUpdatedDate = DateTime.UtcNow,
            IsActive = true
        };
        _context.Banks.Add(bank);
        await _context.SaveChangesAsync();

        var updateRequest = new UpdateBankRequest
        {
            Name = "Updated Bank",
            Description = "Updated description",
            WebUrl = "https://updatedbank.com"
        };

        // Act
        var result = await _service.UpdateAsync(bank.Id, updateRequest);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Updated Bank");
        result.Description.Should().Be("Updated description");
        result.WebUrl.Should().Be("https://updatedbank.com");
    }

    [Fact]
    public async Task UpdateAsync_NonExistentEntity_ReturnsNull()
    {
        // Arrange
        var updateRequest = new UpdateBankRequest
        {
            Name = "Updated Bank"
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
        var bank = new Bank
        {
            Name = "Test Bank",
            CreatedDate = DateTime.UtcNow,
            LastUpdatedDate = DateTime.UtcNow,
            IsActive = true
        };
        _context.Banks.Add(bank);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.SoftDeleteAsync(bank.Id);

        // Assert
        result.Should().BeTrue();
        var deleted = await _context.Banks.FindAsync(bank.Id);
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
        var bank = new Bank
        {
            Name = "Test Bank",
            CreatedDate = DateTime.UtcNow,
            LastUpdatedDate = DateTime.UtcNow,
            IsActive = true
        };
        _context.Banks.Add(bank);
        await _context.SaveChangesAsync();
        var createdId = bank.Id;

        // Act
        var result = await _service.HardDeleteAsync(createdId);

        // Assert
        result.Should().BeTrue();
        var exists = await _context.Banks.AnyAsync(b => b.Id == createdId);
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
