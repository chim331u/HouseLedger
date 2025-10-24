using Bogus;
using HouseLedger.Services.Ancillary.Application.Contracts.Countries;
using HouseLedger.Services.Ancillary.Application.Contracts.Currencies;
using HouseLedger.Services.Ancillary.Application.Contracts.CurrencyConversionRates;
using HouseLedger.Services.Ancillary.Application.Contracts.ServiceUsers;
using HouseLedger.Services.Ancillary.Application.Contracts.Suppliers;
using HouseLedger.Services.Ancillary.Domain.Entities;

namespace HouseLedger.Services.Ancillary.IntegrationTests.Fixtures;

/// <summary>
/// Test data builder using Bogus library to generate fake data for integration testing.
/// </summary>
public static class TestDataBuilder
{
    private static readonly Faker Faker = new();

    #region Country Test Data

    public static CreateCountryRequest CreateCountryRequest(
        string? name = null,
        string? countryCodeAlf3 = null,
        string? countryCodeNum3 = null,
        string? description = null)
    {
        return new CreateCountryRequest
        {
            Name = name ?? Faker.Address.Country(),
            CountryCodeAlf3 = countryCodeAlf3 ?? Faker.Address.CountryCode(),
            CountryCodeNum3 = countryCodeNum3 ?? Faker.Random.Number(100, 999).ToString(),
            Description = description ?? Faker.Lorem.Sentence()
        };
    }

    public static UpdateCountryRequest UpdateCountryRequest(
        string? name = null,
        string? countryCodeAlf3 = null,
        string? countryCodeNum3 = null,
        string? description = null,
        string? note = null)
    {
        return new UpdateCountryRequest
        {
            Name = name ?? Faker.Address.Country(),
            CountryCodeAlf3 = countryCodeAlf3 ?? Faker.Address.CountryCode(),
            CountryCodeNum3 = countryCodeNum3 ?? Faker.Random.Number(100, 999).ToString(),
            Description = description ?? Faker.Lorem.Sentence(),
            Note = note
        };
    }

    public static Country Country(
        int? id = null,
        string? name = null,
        string? countryCodeAlf3 = null,
        DateTime? createdDate = null,
        bool? isActive = null)
    {
        return new Country
        {
            Id = id ?? Faker.Random.Int(1, 1000),
            Name = name ?? Faker.Address.Country(),
            CountryCodeAlf3 = countryCodeAlf3 ?? Faker.Address.CountryCode(),
            CountryCodeNum3 = Faker.Random.Number(100, 999).ToString(),
            Description = Faker.Lorem.Sentence(),
            CreatedDate = createdDate ?? DateTime.UtcNow,
            LastUpdatedDate = DateTime.UtcNow,
            IsActive = isActive ?? true
        };
    }

    #endregion

    #region Currency Test Data

    public static CreateCurrencyRequest CreateCurrencyRequest(
        string? name = null,
        string? currencyCodeAlf3 = null)
    {
        return new CreateCurrencyRequest
        {
            Name = name ?? Faker.Finance.Currency().Description,
            CurrencyCodeAlf3 = currencyCodeAlf3 ?? Faker.Finance.Currency().Code,
            CurrencyCodeNum3 = Faker.Random.Number(100, 999).ToString(),
            Description = Faker.Lorem.Sentence()
        };
    }

    public static Currency Currency(
        int? id = null,
        string? name = null,
        string? currencyCodeAlf3 = null,
        bool? isActive = null)
    {
        return new Currency
        {
            Id = id ?? Faker.Random.Int(1, 1000),
            Name = name ?? Faker.Finance.Currency().Description,
            CurrencyCodeAlf3 = currencyCodeAlf3 ?? Faker.Finance.Currency().Code,
            CurrencyCodeNum3 = Faker.Random.Number(100, 999).ToString(),
            CreatedDate = DateTime.UtcNow,
            LastUpdatedDate = DateTime.UtcNow,
            IsActive = isActive ?? true
        };
    }

    #endregion

    #region CurrencyConversionRate Test Data

    public static CreateCurrencyConversionRateRequest CreateCurrencyConversionRateRequest(
        string? currencyCodeAlf3 = null,
        decimal? rateValue = null,
        DateTime? referringDate = null)
    {
        return new CreateCurrencyConversionRateRequest
        {
            CurrencyCodeAlf3 = currencyCodeAlf3 ?? Faker.Finance.Currency().Code,
            RateValue = rateValue ?? Faker.Finance.Amount(0.01m, 10m),
            ReferringDate = referringDate ?? Faker.Date.Recent(),
            UniqueKey = Guid.NewGuid().ToString()
        };
    }

    public static CurrencyConversionRate CurrencyConversionRate(
        int? id = null,
        string? currencyCodeAlf3 = null,
        decimal? rateValue = null,
        bool? isActive = null)
    {
        return new CurrencyConversionRate
        {
            Id = id ?? Faker.Random.Int(1, 1000),
            CurrencyCodeAlf3 = currencyCodeAlf3 ?? Faker.Finance.Currency().Code,
            RateValue = rateValue ?? Faker.Finance.Amount(0.01m, 10m),
            ReferringDate = Faker.Date.Recent(),
            UniqueKey = Guid.NewGuid().ToString(),
            CreatedDate = DateTime.UtcNow,
            LastUpdatedDate = DateTime.UtcNow,
            IsActive = isActive ?? true
        };
    }

    #endregion

    #region Supplier Test Data

    public static CreateSupplierRequest CreateSupplierRequest(
        string? name = null,
        string? type = null)
    {
        return new CreateSupplierRequest
        {
            Name = name ?? Faker.Company.CompanyName(),
            Type = type ?? Faker.Commerce.Department(),
            Description = Faker.Lorem.Sentence(),
            UnitMeasure = Faker.Commerce.Product(),
            Contract = Faker.Random.AlphaNumeric(10)
        };
    }

    public static Supplier Supplier(
        int? id = null,
        string? name = null,
        bool? isActive = null)
    {
        return new Supplier
        {
            Id = id ?? Faker.Random.Int(1, 1000),
            Name = name ?? Faker.Company.CompanyName(),
            Type = Faker.Commerce.Department(),
            Description = Faker.Lorem.Sentence(),
            CreatedDate = DateTime.UtcNow,
            LastUpdatedDate = DateTime.UtcNow,
            IsActive = isActive ?? true
        };
    }

    #endregion

    #region ServiceUser Test Data

    public static CreateServiceUserRequest CreateServiceUserRequest(
        string? name = null,
        string? surname = null,
        string? note = null)
    {
        return new CreateServiceUserRequest
        {
            Name = name ?? Faker.Name.FirstName(),
            Surname = surname ?? Faker.Name.LastName(),
            Note = note
        };
    }

    public static UpdateServiceUserRequest UpdateServiceUserRequest(
        string? name = null,
        string? surname = null,
        string? note = null)
    {
        return new UpdateServiceUserRequest
        {
            Id = Faker.Random.Int(1, 1000),
            Name = name ?? Faker.Name.FirstName(),
            Surname = surname ?? Faker.Name.LastName(),
            Note = note
        };
    }

    public static ServiceUser ServiceUser(
        int? id = null,
        string? name = null,
        string? surname = null,
        DateTime? createdDate = null,
        bool? isActive = null)
    {
        return new ServiceUser
        {
            Id = id ?? Faker.Random.Int(1, 1000),
            Name = name ?? Faker.Name.FirstName(),
            Surname = surname ?? Faker.Name.LastName(),
            CreatedDate = createdDate ?? DateTime.UtcNow,
            LastUpdatedDate = DateTime.UtcNow,
            IsActive = isActive ?? true
        };
    }

    #endregion
}
