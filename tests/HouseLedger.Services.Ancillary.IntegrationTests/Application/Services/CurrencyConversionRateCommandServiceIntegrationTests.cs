using AutoMapper;
using HouseLedger.Services.Ancillary.Application.Mapping;
using HouseLedger.Services.Ancillary.Application.Services;
using HouseLedger.Services.Ancillary.Application.Contracts.CurrencyConversionRates;
using HouseLedger.Services.Ancillary.IntegrationTests.Fixtures;
using HouseLedger.Services.Ancillary.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HouseLedger.Services.Ancillary.IntegrationTests.Application.Services;

/// <summary>
/// Integration tests for CurrencyConversionRateCommandService with real database operations.
/// Tests decimal precision, date handling, and unique key constraints.
/// </summary>
public class CurrencyConversionRateCommandServiceIntegrationTests : IntegrationTestBase
{
    private readonly IMapper _mapper;
    private readonly CurrencyConversionRateCommandService _service;
    private readonly Mock<ILogger<CurrencyConversionRateCommandService>> _loggerMock;

    public CurrencyConversionRateCommandServiceIntegrationTests()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<AncillaryMappingProfile>();
        });
        _mapper = config.CreateMapper();

        _loggerMock = new Mock<ILogger<CurrencyConversionRateCommandService>>();
        _service = new CurrencyConversionRateCommandService(Context, _mapper, _loggerMock.Object);
    }

    [Fact]
    public async Task CreateAsync_ValidRequest_PersistsToDatabase()
    {
        // Arrange
        var request = TestDataBuilder.CreateCurrencyConversionRateRequest(
            currencyCodeAlf3: "EUR",
            rateValue: 1.18m,
            referringDate: new DateTime(2025, 1, 15));

        // Act
        var result = await _service.CreateAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.CurrencyCodeAlf3.Should().Be("EUR");
        result.RateValue.Should().Be(1.18m);

        Context.ChangeTracker.Clear();
        var persisted = await Context.CurrencyConversionRates.FindAsync(result.Id);
        persisted.Should().NotBeNull();
        persisted!.RateValue.Should().Be(1.18m);
        persisted.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task CreateAsync_HighPrecisionDecimal_PreservesAllDecimals()
    {
        // Arrange
        var highPrecisionRate = 1.23456789m;
        var request = TestDataBuilder.CreateCurrencyConversionRateRequest(
            rateValue: highPrecisionRate);

        // Act
        var result = await _service.CreateAsync(request);

        // Assert
        Context.ChangeTracker.Clear();
        var persisted = await Context.CurrencyConversionRates.FindAsync(result.Id);
        persisted!.RateValue.Should().Be(highPrecisionRate);
    }

    [Fact]
    public async Task CreateAsync_MultipleRatesForSameCurrency_AllPersisted()
    {
        // Arrange
        var rate1 = TestDataBuilder.CreateCurrencyConversionRateRequest(
            currencyCodeAlf3: "GBP",
            rateValue: 1.35m,
            referringDate: new DateTime(2025, 1, 1));

        var rate2 = TestDataBuilder.CreateCurrencyConversionRateRequest(
            currencyCodeAlf3: "GBP",
            rateValue: 1.37m,
            referringDate: new DateTime(2025, 1, 15));

        // Act
        await _service.CreateAsync(rate1);
        await _service.CreateAsync(rate2);

        // Assert
        var rates = await Context.CurrencyConversionRates
            .Where(r => r.CurrencyCodeAlf3 == "GBP")
            .ToListAsync();

        rates.Should().HaveCount(2);
        rates.Should().Contain(r => r.RateValue == 1.35m);
        rates.Should().Contain(r => r.RateValue == 1.37m);
    }

    [Fact]
    public async Task CreateAsync_DifferentCurrencies_AllPersisted()
    {
        // Arrange
        var eur = TestDataBuilder.CreateCurrencyConversionRateRequest(currencyCodeAlf3: "EUR");
        var gbp = TestDataBuilder.CreateCurrencyConversionRateRequest(currencyCodeAlf3: "GBP");
        var jpy = TestDataBuilder.CreateCurrencyConversionRateRequest(currencyCodeAlf3: "JPY");

        // Act
        await _service.CreateAsync(eur);
        await _service.CreateAsync(gbp);
        await _service.CreateAsync(jpy);

        // Assert
        var rates = await Context.CurrencyConversionRates.ToListAsync();
        rates.Should().HaveCount(3);
        rates.Select(r => r.CurrencyCodeAlf3).Should().Contain(new[] { "EUR", "GBP", "JPY" });
    }

    [Fact]
    public async Task UpdateAsync_ExistingRate_PersistsChanges()
    {
        // Arrange
        var createRequest = TestDataBuilder.CreateCurrencyConversionRateRequest(
            rateValue: 1.20m);
        var created = await _service.CreateAsync(createRequest);

        var updateRequest = new UpdateCurrencyConversionRateRequest
        {
            CurrencyCodeAlf3 = "EUR",
            RateValue = 1.25m,
            ReferringDate = new DateTime(2025, 2, 1),
            UniqueKey = Guid.NewGuid().ToString(),
            Note = "Updated rate"
        };

        // Act
        var result = await _service.UpdateAsync(created.Id, updateRequest);

        // Assert
        result.Should().NotBeNull();
        result!.RateValue.Should().Be(1.25m);

        Context.ChangeTracker.Clear();
        var persisted = await Context.CurrencyConversionRates.FindAsync(created.Id);
        persisted!.RateValue.Should().Be(1.25m);
        persisted.Note.Should().Be("Updated rate");
    }

    [Fact]
    public async Task UpdateAsync_ChangeReferringDate_Persists()
    {
        // Arrange
        var originalDate = new DateTime(2025, 1, 1);
        var newDate = new DateTime(2025, 3, 1);

        var createRequest = TestDataBuilder.CreateCurrencyConversionRateRequest(
            referringDate: originalDate);
        var created = await _service.CreateAsync(createRequest);

        var updateRequest = new UpdateCurrencyConversionRateRequest
        {
            CurrencyCodeAlf3 = created.CurrencyCodeAlf3,
            RateValue = created.RateValue,
            ReferringDate = newDate,
            UniqueKey = Guid.NewGuid().ToString()
        };

        // Act
        await _service.UpdateAsync(created.Id, updateRequest);

        // Assert
        Context.ChangeTracker.Clear();
        var persisted = await Context.CurrencyConversionRates.FindAsync(created.Id);
        persisted!.ReferringDate.Date.Should().Be(newDate.Date);
    }

    [Fact]
    public async Task SoftDeleteAsync_ExistingRate_SetsIsActiveFalse()
    {
        // Arrange
        var createRequest = TestDataBuilder.CreateCurrencyConversionRateRequest();
        var created = await _service.CreateAsync(createRequest);

        // Act
        var result = await _service.SoftDeleteAsync(created.Id);

        // Assert
        result.Should().BeTrue();

        Context.ChangeTracker.Clear();
        var deleted = await Context.CurrencyConversionRates.FindAsync(created.Id);
        deleted!.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task HardDeleteAsync_ExistingRate_RemovesFromDatabase()
    {
        // Arrange
        var createRequest = TestDataBuilder.CreateCurrencyConversionRateRequest();
        var created = await _service.CreateAsync(createRequest);

        // Act
        var result = await _service.HardDeleteAsync(created.Id);

        // Assert
        result.Should().BeTrue();

        Context.ChangeTracker.Clear();
        var exists = await Context.CurrencyConversionRates.AnyAsync(r => r.Id == created.Id);
        exists.Should().BeFalse();
    }

    [Fact]
    public async Task FullCRUDCycle_CurrencyConversionRate_WorksEndToEnd()
    {
        // Create
        var createRequest = TestDataBuilder.CreateCurrencyConversionRateRequest(
            currencyCodeAlf3: "CHF",
            rateValue: 0.92m);
        var created = await _service.CreateAsync(createRequest);
        created.RateValue.Should().Be(0.92m);

        // Update
        var updateRequest = new UpdateCurrencyConversionRateRequest
        {
            CurrencyCodeAlf3 = "CHF",
            RateValue = 0.95m,
            ReferringDate = DateTime.UtcNow,
            UniqueKey = Guid.NewGuid().ToString(),
            Note = "Swiss Franc rate"
        };
        var updated = await _service.UpdateAsync(created.Id, updateRequest);
        updated!.RateValue.Should().Be(0.95m);

        // Soft Delete
        await _service.SoftDeleteAsync(created.Id);
        Context.ChangeTracker.Clear();
        var softDeleted = await Context.CurrencyConversionRates.FindAsync(created.Id);
        softDeleted!.IsActive.Should().BeFalse();

        // Hard Delete
        await _service.HardDeleteAsync(created.Id);
        Context.ChangeTracker.Clear();
        var hardDeleted = await Context.CurrencyConversionRates.FindAsync(created.Id);
        hardDeleted.Should().BeNull();
    }
}
