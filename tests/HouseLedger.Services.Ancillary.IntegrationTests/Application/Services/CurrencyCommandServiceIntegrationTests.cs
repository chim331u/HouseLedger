using AutoMapper;
using HouseLedger.Services.Ancillary.Application.Mapping;
using HouseLedger.Services.Ancillary.Application.Services;
using HouseLedger.Services.Ancillary.Application.Contracts.Currencies;
using HouseLedger.Services.Ancillary.IntegrationTests.Fixtures;
using HouseLedger.Services.Ancillary.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HouseLedger.Services.Ancillary.IntegrationTests.Application.Services;

/// <summary>
/// Integration tests for CurrencyCommandService with real database operations.
/// </summary>
public class CurrencyCommandServiceIntegrationTests : IntegrationTestBase
{
    private readonly IMapper _mapper;
    private readonly CurrencyCommandService _service;
    private readonly Mock<ILogger<CurrencyCommandService>> _loggerMock;

    public CurrencyCommandServiceIntegrationTests()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<AncillaryMappingProfile>();
        });
        _mapper = config.CreateMapper();

        _loggerMock = new Mock<ILogger<CurrencyCommandService>>();
        _service = new CurrencyCommandService(Context, _mapper, _loggerMock.Object);
    }

    [Fact]
    public async Task CreateAsync_ValidRequest_PersistsToDatabase()
    {
        // Arrange
        var request = TestDataBuilder.CreateCurrencyRequest(
            name: "US Dollar",
            currencyCodeAlf3: "USD");

        // Act
        var result = await _service.CreateAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("US Dollar");
        result.CurrencyCodeAlf3.Should().Be("USD");

        Context.ChangeTracker.Clear();
        var persisted = await Context.Currencies.FindAsync(result.Id);
        persisted.Should().NotBeNull();
        persisted!.Name.Should().Be("US Dollar");
        persisted.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task CreateAsync_MultipleCurrencies_AllPersisted()
    {
        // Arrange
        var usd = TestDataBuilder.CreateCurrencyRequest(name: "US Dollar", currencyCodeAlf3: "USD");
        var eur = TestDataBuilder.CreateCurrencyRequest(name: "Euro", currencyCodeAlf3: "EUR");
        var gbp = TestDataBuilder.CreateCurrencyRequest(name: "British Pound", currencyCodeAlf3: "GBP");

        // Act
        await _service.CreateAsync(usd);
        await _service.CreateAsync(eur);
        await _service.CreateAsync(gbp);

        // Assert
        var currencies = await Context.Currencies.ToListAsync();
        currencies.Should().HaveCount(3);
        currencies.Should().Contain(c => c.CurrencyCodeAlf3 == "USD");
        currencies.Should().Contain(c => c.CurrencyCodeAlf3 == "EUR");
        currencies.Should().Contain(c => c.CurrencyCodeAlf3 == "GBP");
    }

    [Fact]
    public async Task UpdateAsync_ExistingCurrency_PersistsChanges()
    {
        // Arrange
        var createRequest = TestDataBuilder.CreateCurrencyRequest(name: "Original");
        var created = await _service.CreateAsync(createRequest);

        var updateRequest = new UpdateCurrencyRequest
        {
            Name = "Updated",
            CurrencyCodeAlf3 = "UPD",
            CurrencyCodeNum3 = "999",
            Description = "Updated description",
            Note = "Updated note"
        };

        // Act
        var result = await _service.UpdateAsync(created.Id, updateRequest);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Updated");

        Context.ChangeTracker.Clear();
        var persisted = await Context.Currencies.FindAsync(created.Id);
        persisted!.Name.Should().Be("Updated");
        persisted.Note.Should().Be("Updated note");
    }

    [Fact]
    public async Task SoftDeleteAsync_ExistingCurrency_SetsIsActiveFalse()
    {
        // Arrange
        var createRequest = TestDataBuilder.CreateCurrencyRequest();
        var created = await _service.CreateAsync(createRequest);

        // Act
        var result = await _service.SoftDeleteAsync(created.Id);

        // Assert
        result.Should().BeTrue();

        Context.ChangeTracker.Clear();
        var deleted = await Context.Currencies.FindAsync(created.Id);
        deleted!.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task HardDeleteAsync_ExistingCurrency_RemovesFromDatabase()
    {
        // Arrange
        var createRequest = TestDataBuilder.CreateCurrencyRequest();
        var created = await _service.CreateAsync(createRequest);

        // Act
        var result = await _service.HardDeleteAsync(created.Id);

        // Assert
        result.Should().BeTrue();

        Context.ChangeTracker.Clear();
        var exists = await Context.Currencies.AnyAsync(c => c.Id == created.Id);
        exists.Should().BeFalse();
    }

    [Fact]
    public async Task FullCRUDCycle_Currency_WorksEndToEnd()
    {
        // Create
        var createRequest = TestDataBuilder.CreateCurrencyRequest(name: "Japanese Yen");
        var created = await _service.CreateAsync(createRequest);
        created.Name.Should().Be("Japanese Yen");

        // Update
        var updateRequest = new UpdateCurrencyRequest
        {
            Name = "JPY - Japanese Yen",
            CurrencyCodeAlf3 = "JPY",
            CurrencyCodeNum3 = "392",
            Description = "Japanese currency"
        };
        var updated = await _service.UpdateAsync(created.Id, updateRequest);
        updated!.Name.Should().Be("JPY - Japanese Yen");

        // Soft Delete
        await _service.SoftDeleteAsync(created.Id);
        Context.ChangeTracker.Clear();
        var softDeleted = await Context.Currencies.FindAsync(created.Id);
        softDeleted!.IsActive.Should().BeFalse();

        // Hard Delete
        await _service.HardDeleteAsync(created.Id);
        Context.ChangeTracker.Clear();
        var hardDeleted = await Context.Currencies.FindAsync(created.Id);
        hardDeleted.Should().BeNull();
    }
}
