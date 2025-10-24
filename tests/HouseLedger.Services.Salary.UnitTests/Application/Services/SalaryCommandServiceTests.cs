using AutoMapper;
using FluentAssertions;
using HouseLedger.Services.Ancillary.Application.Contracts.Currencies;
using HouseLedger.Services.Ancillary.Application.Contracts.CurrencyConversionRates;
using HouseLedger.Services.Ancillary.Application.Interfaces;
using HouseLedger.Services.Salary.Application.Contracts.Salaries;
using HouseLedger.Services.Salary.Application.Mapping;
using HouseLedger.Services.Salary.Application.Services;
using HouseLedger.Services.Salary.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace HouseLedger.Services.Salary.UnitTests.Application.Services;

/// <summary>
/// Unit tests for SalaryCommandService.
/// Tests creation, update, and deletion of salary entries.
/// </summary>
public class SalaryCommandServiceTests : IDisposable
{
    private readonly SalaryDbContext _context;
    private readonly IMapper _mapper;
    private readonly Mock<ILogger<SalaryCommandService>> _loggerMock;
    private readonly Mock<ICurrencyQueryService> _currencyQueryServiceMock;
    private readonly Mock<ICurrencyConversionRateQueryService> _conversionRateQueryServiceMock;
    private readonly SalaryCommandService _service;

    public SalaryCommandServiceTests()
    {
        // Setup in-memory database
        var options = new DbContextOptionsBuilder<SalaryDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new SalaryDbContext(options);

        // Setup AutoMapper
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<SalaryMappingProfile>();
        });
        _mapper = config.CreateMapper();

        // Setup logger mock
        _loggerMock = new Mock<ILogger<SalaryCommandService>>();

        // Setup currency service mocks
        _currencyQueryServiceMock = new Mock<ICurrencyQueryService>();
        _conversionRateQueryServiceMock = new Mock<ICurrencyConversionRateQueryService>();

        // Setup default mock behavior: return EUR currency with rate 1.0
        _currencyQueryServiceMock
            .Setup(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CurrencyDto
            {
                Id = 1,
                Name = "Euro",
                CurrencyCodeAlf3 = "EUR",
                CurrencyCodeNum3 = "978"
            });

        // Create service
        _service = new SalaryCommandService(
            _context,
            _mapper,
            _loggerMock.Object,
            _currencyQueryServiceMock.Object,
            _conversionRateQueryServiceMock.Object);
    }

    [Fact]
    public async Task CreateAsync_ValidRequest_ReturnsDto()
    {
        // Arrange
        var request = new CreateSalaryRequest
        {
            SalaryValue = 5000.00m,
            SalaryDate = new DateTime(2024, 10, 15),
            ReferYear = "2024",
            ReferMonth = "October",
            CurrencyId = 1,
            UserId = 1
        };

        // Act
        var result = await _service.CreateAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
        result.SalaryValue.Should().Be(5000.00m);
        result.SalaryDate.Should().Be(new DateTime(2024, 10, 15));
        result.ReferYear.Should().Be("2024");
        result.ReferMonth.Should().Be("October");
        result.CurrencyId.Should().Be(1);
        result.UserId.Should().Be(1);
        result.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task CreateAsync_ValidRequest_SetsAuditFields()
    {
        // Arrange
        var request = new CreateSalaryRequest
        {
            SalaryValue = 5000.00m,
            SalaryDate = DateTime.Now,
            CurrencyId = 1,
            UserId = 1
        };

        var beforeCreation = DateTime.UtcNow;

        // Act
        var result = await _service.CreateAsync(request);

        var afterCreation = DateTime.UtcNow;

        // Assert
        result.CreatedDate.Should().BeOnOrAfter(beforeCreation);
        result.CreatedDate.Should().BeOnOrBefore(afterCreation);
        result.LastUpdatedDate.Should().BeOnOrAfter(beforeCreation);
        result.LastUpdatedDate.Should().BeOnOrBefore(afterCreation);
        result.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task CreateAsync_ValidRequest_CalculatesEurValue()
    {
        // Arrange
        var request = new CreateSalaryRequest
        {
            SalaryValue = 5000.00m,
            SalaryDate = DateTime.Now,
            CurrencyId = 1,
            UserId = 1
        };

        // Act
        var result = await _service.CreateAsync(request);

        // Assert (EUR value should equal salary value when exchange rate is 1.0)
        result.SalaryValueEur.Should().Be(5000.00m);
        result.ExchangeRate.Should().Be(1.0m);
    }

    [Fact]
    public async Task UpdateAsync_ExistingEntity_ReturnsUpdatedDto()
    {
        // Arrange
        var createRequest = new CreateSalaryRequest
        {
            SalaryValue = 5000.00m,
            SalaryDate = new DateTime(2024, 10, 15),
            CurrencyId = 1,
            UserId = 1
        };

        var created = await _service.CreateAsync(createRequest);

        var updateRequest = new UpdateSalaryRequest
        {
            Id = created.Id,
            SalaryValue = 6000.00m,
            SalaryDate = new DateTime(2024, 11, 15),
            ReferYear = "2024",
            ReferMonth = "November",
            CurrencyId = 2,
            UserId = 2
        };

        // Act
        var result = await _service.UpdateAsync(created.Id, updateRequest);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(created.Id);
        result.SalaryValue.Should().Be(6000.00m);
        result.SalaryDate.Should().Be(new DateTime(2024, 11, 15));
        result.ReferYear.Should().Be("2024");
        result.ReferMonth.Should().Be("November");
        result.CurrencyId.Should().Be(2);
        result.UserId.Should().Be(2);
    }

    [Fact]
    public async Task UpdateAsync_NonExistentEntity_ReturnsNull()
    {
        // Arrange
        var updateRequest = new UpdateSalaryRequest
        {
            Id = 999,
            SalaryValue = 6000.00m,
            SalaryDate = DateTime.Now,
            CurrencyId = 1,
            UserId = 1
        };

        // Act
        var result = await _service.UpdateAsync(999, updateRequest);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task SoftDeleteAsync_ExistingEntity_SetsIsActiveFalse()
    {
        // Arrange
        var createRequest = new CreateSalaryRequest
        {
            SalaryValue = 5000.00m,
            SalaryDate = DateTime.Now,
            CurrencyId = 1,
            UserId = 1
        };

        var created = await _service.CreateAsync(createRequest);

        // Act
        var result = await _service.SoftDeleteAsync(created.Id);

        // Assert
        result.Should().BeTrue();

        var deletedEntity = await _context.Salaries.FindAsync(created.Id);
        deletedEntity.Should().NotBeNull();
        deletedEntity!.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task SoftDeleteAsync_NonExistentEntity_ReturnsFalse()
    {
        // Arrange & Act
        var result = await _service.SoftDeleteAsync(999);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task HardDeleteAsync_ExistingEntity_RemovesFromDatabase()
    {
        // Arrange
        var createRequest = new CreateSalaryRequest
        {
            SalaryValue = 5000.00m,
            SalaryDate = DateTime.Now,
            CurrencyId = 1,
            UserId = 1
        };

        var created = await _service.CreateAsync(createRequest);

        // Act
        var result = await _service.HardDeleteAsync(created.Id);

        // Assert
        result.Should().BeTrue();

        var deletedEntity = await _context.Salaries.FindAsync(created.Id);
        deletedEntity.Should().BeNull();
    }

    [Fact]
    public async Task HardDeleteAsync_NonExistentEntity_ReturnsFalse()
    {
        // Arrange & Act
        var result = await _service.HardDeleteAsync(999);

        // Assert
        result.Should().BeFalse();
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
