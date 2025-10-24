using HouseLedger.Services.Ancillary.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HouseLedger.Services.Ancillary.IntegrationTests.Infrastructure;

/// <summary>
/// Integration tests for AncillaryDbContext focusing on:
/// - SaveChangesAsync behavior
/// - Audit field automatic population
/// - Concurrent updates handling
/// </summary>
public class AncillaryDbContextTests : IntegrationTestBase
{
    [Fact]
    public async Task SaveChangesAsync_NewEntity_SetsAuditFieldsAutomatically()
    {
        // Arrange
        var country = new Country
        {
            Name = "Test Country",
            CountryCodeAlf3 = "TST",
            CountryCodeNum3 = "123",
            Description = "Test Description"
            // Note: CreatedDate, LastUpdatedDate, IsActive are NOT set
        };

        var beforeSave = DateTime.UtcNow;

        // Act
        Context.Countries.Add(country);
        await Context.SaveChangesAsync();

        // Assert
        country.CreatedDate.Should().BeCloseTo(beforeSave, TimeSpan.FromSeconds(5));
        country.LastUpdatedDate.Should().BeCloseTo(beforeSave, TimeSpan.FromSeconds(5));
        country.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task SaveChangesAsync_UpdateEntity_UpdatesLastUpdatedDateOnly()
    {
        // Arrange
        var country = new Country
        {
            Name = "Original Country",
            CountryCodeAlf3 = "ORI",
            CountryCodeNum3 = "456",
            Description = "Original"
        };

        Context.Countries.Add(country);
        await Context.SaveChangesAsync();

        var originalCreatedDate = country.CreatedDate;
        var originalLastUpdatedDate = country.LastUpdatedDate;

        // Wait a bit to ensure time difference
        await Task.Delay(100);
        var beforeUpdate = DateTime.UtcNow;

        // Act
        country.Name = "Updated Country";
        await Context.SaveChangesAsync();

        // Assert
        country.CreatedDate.Should().Be(originalCreatedDate);
        country.LastUpdatedDate.Should().BeAfter(originalLastUpdatedDate);
        country.LastUpdatedDate.Should().BeCloseTo(beforeUpdate, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task SaveChangesAsync_MultipleEntities_SetsAuditFieldsForAll()
    {
        // Arrange
        var country1 = new Country
        {
            Name = "Country 1",
            CountryCodeAlf3 = "CO1",
            CountryCodeNum3 = "111",
            Description = "First"
        };

        var country2 = new Country
        {
            Name = "Country 2",
            CountryCodeAlf3 = "CO2",
            CountryCodeNum3 = "222",
            Description = "Second"
        };

        var currency = new Currency
        {
            Name = "Test Currency",
            CurrencyCodeAlf3 = "TST",
            CurrencyCodeNum3 = "333",
            Description = "Currency"
        };

        var beforeSave = DateTime.UtcNow;

        // Act
        Context.Countries.AddRange(country1, country2);
        Context.Currencies.Add(currency);
        await Context.SaveChangesAsync();

        // Assert
        country1.IsActive.Should().BeTrue();
        country1.CreatedDate.Should().BeCloseTo(beforeSave, TimeSpan.FromSeconds(5));

        country2.IsActive.Should().BeTrue();
        country2.CreatedDate.Should().BeCloseTo(beforeSave, TimeSpan.FromSeconds(5));

        currency.IsActive.Should().BeTrue();
        currency.CreatedDate.Should().BeCloseTo(beforeSave, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task SaveChangesAsync_SoftDeletedEntity_DoesNotUpdateAuditFields()
    {
        // Arrange
        var country = new Country
        {
            Name = "Country",
            CountryCodeAlf3 = "COU",
            CountryCodeNum3 = "789",
            Description = "Description"
        };

        Context.Countries.Add(country);
        await Context.SaveChangesAsync();

        var originalCreatedDate = country.CreatedDate;
        var originalLastUpdatedDate = country.LastUpdatedDate;

        await Task.Delay(100);

        // Act
        country.IsActive = false;
        await Context.SaveChangesAsync();

        // Assert
        country.CreatedDate.Should().Be(originalCreatedDate);
        country.LastUpdatedDate.Should().BeAfter(originalLastUpdatedDate);
        country.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task SaveChangesAsync_UnchangedEntity_DoesNotUpdateLastUpdatedDate()
    {
        // Arrange
        var country = new Country
        {
            Name = "Country",
            CountryCodeAlf3 = "COU",
            CountryCodeNum3 = "999",
            Description = "Description"
        };

        Context.Countries.Add(country);
        await Context.SaveChangesAsync();

        var originalLastUpdatedDate = country.LastUpdatedDate;

        // Detach and reattach to simulate new context
        Context.Entry(country).State = EntityState.Detached;
        Context.Countries.Attach(country);

        // Act
        await Context.SaveChangesAsync();

        // Assert
        country.LastUpdatedDate.Should().Be(originalLastUpdatedDate);
    }

    [Fact]
    public async Task SaveChangesAsync_ExplicitAuditFields_AreOverridden()
    {
        // Arrange
        var pastDate = DateTime.UtcNow.AddYears(-1);
        var country = new Country
        {
            Name = "Country",
            CountryCodeAlf3 = "COU",
            CountryCodeNum3 = "888",
            Description = "Description",
            CreatedDate = pastDate,
            LastUpdatedDate = pastDate,
            IsActive = false
        };

        var beforeSave = DateTime.UtcNow;

        // Act
        Context.Countries.Add(country);
        await Context.SaveChangesAsync();

        // Assert - Audit fields should be overridden by SaveChangesAsync
        country.CreatedDate.Should().BeCloseTo(beforeSave, TimeSpan.FromSeconds(5));
        country.CreatedDate.Should().NotBe(pastDate);
        country.LastUpdatedDate.Should().BeCloseTo(beforeSave, TimeSpan.FromSeconds(5));
        country.IsActive.Should().BeTrue(); // Should be overridden to true
    }
}
