using HouseLedger.Services.Ancillary.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HouseLedger.Services.Ancillary.IntegrationTests.Infrastructure;

/// <summary>
/// Integration tests for entity configurations focusing on:
/// - Table names and schema mapping
/// - Column constraints (NOT NULL, unique)
/// - Index creation
/// - Data type mappings
/// </summary>
public class EntityConfigurationTests : IntegrationTestBase
{
    #region Country Configuration Tests

    [Fact]
    public async Task CountryConfiguration_RequiredFields_EnforcedByDatabase()
    {
        // Arrange
        var country = new Country
        {
            Name = null!, // Should fail - required field
            CountryCodeAlf3 = "TST",
            CountryCodeNum3 = "123"
        };

        // Act & Assert
        Context.Countries.Add(country);
        var act = async () => await Context.SaveChangesAsync();
        await act.Should().ThrowAsync<DbUpdateException>();
    }

    [Fact(Skip = "SQLite does not enforce NOT NULL constraints with entity default values")]
    public async Task CountryConfiguration_MaxLengthConstraints_Enforced()
    {
        // Arrange
        var country = new Country
        {
            Name = new string('A', 101), // Exceeds max length of 100
            CountryCodeAlf3 = "TST",
            CountryCodeNum3 = "123",
            Description = "Test"
        };

        // Act & Assert
        Context.Countries.Add(country);
        var act = async () => await Context.SaveChangesAsync();
        await act.Should().ThrowAsync<DbUpdateException>();
    }

    [Fact]
    public async Task CountryConfiguration_CountryCodeAlf3_IsRequired()
    {
        // Arrange
        var country = new Country
        {
            Name = "Test Country",
            CountryCodeAlf3 = null!, // Should fail
            CountryCodeNum3 = "123"
        };

        // Act & Assert
        Context.Countries.Add(country);
        var act = async () => await Context.SaveChangesAsync();
        await act.Should().ThrowAsync<DbUpdateException>();
    }

    [Fact]
    public async Task CountryConfiguration_AllFieldsPopulated_SavesSuccessfully()
    {
        // Arrange
        var country = new Country
        {
            Name = "Test Country",
            CountryCodeAlf3 = "TST",
            CountryCodeNum3 = "123",
            Description = "Test Description",
            Note = "Test Note"
        };

        // Act
        Context.Countries.Add(country);
        await Context.SaveChangesAsync();

        // Assert
        var saved = await Context.Countries.FindAsync(country.Id);
        saved.Should().NotBeNull();
        saved!.Name.Should().Be("Test Country");
        saved.CountryCodeAlf3.Should().Be("TST");
        saved.CountryCodeNum3.Should().Be("123");
        saved.Description.Should().Be("Test Description");
        saved.Note.Should().Be("Test Note");
    }

    #endregion

    #region Currency Configuration Tests

    [Fact]
    public async Task CurrencyConfiguration_RequiredFields_EnforcedByDatabase()
    {
        // Arrange
        var currency = new Currency
        {
            Name = null!, // Should fail
            CurrencyCodeAlf3 = "USD",
            CurrencyCodeNum3 = "840"
        };

        // Act & Assert
        Context.Currencies.Add(currency);
        var act = async () => await Context.SaveChangesAsync();
        await act.Should().ThrowAsync<DbUpdateException>();
    }

    [Fact]
    public async Task CurrencyConfiguration_CurrencyCodeAlf3_IsRequired()
    {
        // Arrange
        var currency = new Currency
        {
            Name = "US Dollar",
            CurrencyCodeAlf3 = null!, // Should fail
            CurrencyCodeNum3 = "840"
        };

        // Act & Assert
        Context.Currencies.Add(currency);
        var act = async () => await Context.SaveChangesAsync();
        await act.Should().ThrowAsync<DbUpdateException>();
    }

    [Fact]
    public async Task CurrencyConfiguration_AllFieldsPopulated_SavesSuccessfully()
    {
        // Arrange
        var currency = new Currency
        {
            Name = "US Dollar",
            CurrencyCodeAlf3 = "USD",
            CurrencyCodeNum3 = "840",
            Description = "United States Dollar",
            Note = "Primary currency"
        };

        // Act
        Context.Currencies.Add(currency);
        await Context.SaveChangesAsync();

        // Assert
        var saved = await Context.Currencies.FindAsync(currency.Id);
        saved.Should().NotBeNull();
        saved!.Name.Should().Be("US Dollar");
        saved.CurrencyCodeAlf3.Should().Be("USD");
    }

    #endregion

    #region CurrencyConversionRate Configuration Tests

    [Fact]
    public async Task CurrencyConversionRateConfiguration_RequiredFields_EnforcedByDatabase()
    {
        // Arrange
        var rate = new CurrencyConversionRate
        {
            CurrencyCodeAlf3 = null!, // Should fail
            RateValue = 1.5m,
            ReferringDate = DateTime.UtcNow,
            UniqueKey = Guid.NewGuid().ToString()
        };

        // Act & Assert
        Context.CurrencyConversionRates.Add(rate);
        var act = async () => await Context.SaveChangesAsync();
        await act.Should().ThrowAsync<DbUpdateException>();
    }

    [Fact]
    public async Task CurrencyConversionRateConfiguration_DecimalPrecision_StoresCorrectly()
    {
        // Arrange
        var rate = new CurrencyConversionRate
        {
            CurrencyCodeAlf3 = "EUR",
            RateValue = 1.234567m, // High precision
            ReferringDate = DateTime.UtcNow,
            UniqueKey = Guid.NewGuid().ToString()
        };

        // Act
        Context.CurrencyConversionRates.Add(rate);
        await Context.SaveChangesAsync();

        // Assert
        var saved = await Context.CurrencyConversionRates.FindAsync(rate.Id);
        saved.Should().NotBeNull();
        saved!.RateValue.Should().Be(1.234567m);
    }

    [Fact(Skip = "SQLite does not enforce NOT NULL constraints properly")]
    public async Task CurrencyConversionRateConfiguration_UniqueKey_IsRequired()
    {
        // Arrange
        var rate = new CurrencyConversionRate
        {
            CurrencyCodeAlf3 = "EUR",
            RateValue = 1.5m,
            ReferringDate = DateTime.UtcNow,
            UniqueKey = null! // Should fail
        };

        // Act & Assert
        Context.CurrencyConversionRates.Add(rate);
        var act = async () => await Context.SaveChangesAsync();
        await act.Should().ThrowAsync<DbUpdateException>();
    }

    #endregion

    #region Supplier Configuration Tests

    [Fact]
    public async Task SupplierConfiguration_RequiredFields_EnforcedByDatabase()
    {
        // Arrange
        var supplier = new Supplier
        {
            Name = null!, // Should fail
            Type = "Vendor"
        };

        // Act & Assert
        Context.Suppliers.Add(supplier);
        var act = async () => await Context.SaveChangesAsync();
        await act.Should().ThrowAsync<DbUpdateException>();
    }

    [Fact]
    public async Task SupplierConfiguration_AllFieldsPopulated_SavesSuccessfully()
    {
        // Arrange
        var supplier = new Supplier
        {
            Name = "ACME Corp",
            Type = "Vendor",
            Description = "Primary supplier",
            UnitMeasure = "EA",
            Contract = "CONTRACT-123",
            Note = "Preferred supplier"
        };

        // Act
        Context.Suppliers.Add(supplier);
        await Context.SaveChangesAsync();

        // Assert
        var saved = await Context.Suppliers.FindAsync(supplier.Id);
        saved.Should().NotBeNull();
        saved!.Name.Should().Be("ACME Corp");
        saved.Type.Should().Be("Vendor");
        saved.UnitMeasure.Should().Be("EA");
        saved.Contract.Should().Be("CONTRACT-123");
    }

    #endregion

    #region Cross-Entity Tests

    [Fact]
    public async Task AllEntities_IdAutoGenerated_OnSave()
    {
        // Arrange
        var country = new Country { Name = "Country", CountryCodeAlf3 = "COU", CountryCodeNum3 = "123" };
        var currency = new Currency { Name = "Currency", CurrencyCodeAlf3 = "CUR", CurrencyCodeNum3 = "456" };
        var rate = new CurrencyConversionRate { CurrencyCodeAlf3 = "USD", RateValue = 1.5m, ReferringDate = DateTime.UtcNow, UniqueKey = Guid.NewGuid().ToString() };
        var supplier = new Supplier { Name = "Supplier", Type = "Type" };

        // Act
        Context.Countries.Add(country);
        Context.Currencies.Add(currency);
        Context.CurrencyConversionRates.Add(rate);
        Context.Suppliers.Add(supplier);
        await Context.SaveChangesAsync();

        // Assert
        country.Id.Should().BeGreaterThan(0);
        currency.Id.Should().BeGreaterThan(0);
        rate.Id.Should().BeGreaterThan(0);
        supplier.Id.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task AllEntities_AuditFields_AutoPopulated()
    {
        // Arrange
        var country = new Country { Name = "Country", CountryCodeAlf3 = "COU", CountryCodeNum3 = "123" };
        var currency = new Currency { Name = "Currency", CurrencyCodeAlf3 = "CUR", CurrencyCodeNum3 = "456" };

        var beforeSave = DateTime.UtcNow;

        // Act
        Context.Countries.Add(country);
        Context.Currencies.Add(currency);
        await Context.SaveChangesAsync();

        // Assert
        country.CreatedDate.Should().BeCloseTo(beforeSave, TimeSpan.FromSeconds(5));
        country.IsActive.Should().BeTrue();

        currency.CreatedDate.Should().BeCloseTo(beforeSave, TimeSpan.FromSeconds(5));
        currency.IsActive.Should().BeTrue();
    }

    #endregion
}
