using AutoMapper;
using FluentAssertions;
using HouseLedger.Services.Finance.Application.Contracts.Accounts;
using HouseLedger.Services.Finance.Application.Mapping;
using HouseLedger.Services.Finance.Application.Services;
using HouseLedger.Services.Finance.Domain.Entities;
using HouseLedger.Services.Finance.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace HouseLedger.Services.Finance.UnitTests.Application.Services;

/// <summary>
/// Unit tests for AccountCommandService.
/// Tests CRUD operations, audit field initialization, and error handling.
/// </summary>
public class AccountCommandServiceTests : IDisposable
{
    private readonly FinanceDbContext _context;
    private readonly IMapper _mapper;
    private readonly AccountCommandService _service;
    private readonly Mock<ILogger<AccountCommandService>> _loggerMock;

    public AccountCommandServiceTests()
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
        _loggerMock = new Mock<ILogger<AccountCommandService>>();

        // Create service
        _service = new AccountCommandService(_context, _mapper, _loggerMock.Object);
    }

    #region CreateAsync Tests

    [Fact]
    public async Task CreateAsync_ValidRequest_ReturnsDto()
    {
        // Arrange
        var request = new CreateAccountRequest
        {
            Name = "Test Account",
            AccountNumber = "1234567890",
            Iban = "IT60X0542811101000000123456",
            Bic = "ABCDITMM",
            AccountType = "Checking",
            CurrencyId = 1,
            BankId = 1
        };

        // Act
        var result = await _service.CreateAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be(request.Name);
        result.AccountNumber.Should().Be(request.AccountNumber);
        result.Iban.Should().Be(request.Iban);
        result.Id.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task CreateAsync_ValidRequest_SetsAuditFields()
    {
        // Arrange
        var request = new CreateAccountRequest
        {
            Name = "Test Account",
            AccountType = "Checking"
        };
        var beforeCreate = DateTime.UtcNow;

        // Act
        var result = await _service.CreateAsync(request);

        // Assert
        var entity = await _context.Accounts.FindAsync(result.Id);
        entity.Should().NotBeNull();
        entity!.CreatedDate.Should().BeCloseTo(beforeCreate, TimeSpan.FromSeconds(5));
        entity.LastUpdatedDate.Should().BeCloseTo(beforeCreate, TimeSpan.FromSeconds(5));
        entity.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task CreateAsync_ValidRequest_SavesToDatabase()
    {
        // Arrange
        var request = new CreateAccountRequest
        {
            Name = "Savings Account",
            AccountType = "Savings"
        };

        // Act
        var result = await _service.CreateAsync(request);

        // Assert
        var entityInDb = await _context.Accounts.FindAsync(result.Id);
        entityInDb.Should().NotBeNull();
        entityInDb!.Name.Should().Be(request.Name);
        entityInDb.AccountType.Should().Be(request.AccountType);
    }

    #endregion

    #region UpdateAsync Tests

    [Fact]
    public async Task UpdateAsync_ExistingEntity_ReturnsUpdatedDto()
    {
        // Arrange
        var account = new Account
        {
            Name = "Original Account",
            AccountType = "Checking",
            CreatedDate = DateTime.UtcNow,
            LastUpdatedDate = DateTime.UtcNow,
            IsActive = true
        };
        _context.Accounts.Add(account);
        await _context.SaveChangesAsync();

        var updateRequest = new UpdateAccountRequest
        {
            Name = "Updated Account",
            AccountType = "Savings",
            Iban = "IT60X0542811101000000123456"
        };

        // Act
        var result = await _service.UpdateAsync(account.Id, updateRequest);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Updated Account");
        result.AccountType.Should().Be("Savings");
        result.Iban.Should().Be("IT60X0542811101000000123456");
    }

    [Fact]
    public async Task UpdateAsync_NonExistentEntity_ReturnsNull()
    {
        // Arrange
        var updateRequest = new UpdateAccountRequest
        {
            Name = "Updated Account"
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
        var account = new Account
        {
            Name = "Test Account",
            CreatedDate = DateTime.UtcNow,
            LastUpdatedDate = DateTime.UtcNow,
            IsActive = true
        };
        _context.Accounts.Add(account);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.SoftDeleteAsync(account.Id);

        // Assert
        result.Should().BeTrue();
        var deleted = await _context.Accounts.FindAsync(account.Id);
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
        var account = new Account
        {
            Name = "Test Account",
            CreatedDate = DateTime.UtcNow,
            LastUpdatedDate = DateTime.UtcNow,
            IsActive = true
        };
        _context.Accounts.Add(account);
        await _context.SaveChangesAsync();
        var createdId = account.Id;

        // Act
        var result = await _service.HardDeleteAsync(createdId);

        // Assert
        result.Should().BeTrue();
        var exists = await _context.Accounts.AnyAsync(a => a.Id == createdId);
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
