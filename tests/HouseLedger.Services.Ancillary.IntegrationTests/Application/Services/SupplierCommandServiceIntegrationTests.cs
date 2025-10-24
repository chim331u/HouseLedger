using AutoMapper;
using HouseLedger.Services.Ancillary.Application.Mapping;
using HouseLedger.Services.Ancillary.Application.Services;
using HouseLedger.Services.Ancillary.Application.Contracts.Suppliers;
using HouseLedger.Services.Ancillary.IntegrationTests.Fixtures;
using HouseLedger.Services.Ancillary.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HouseLedger.Services.Ancillary.IntegrationTests.Application.Services;

/// <summary>
/// Integration tests for SupplierCommandService with real database operations.
/// </summary>
public class SupplierCommandServiceIntegrationTests : IntegrationTestBase
{
    private readonly IMapper _mapper;
    private readonly SupplierCommandService _service;
    private readonly Mock<ILogger<SupplierCommandService>> _loggerMock;

    public SupplierCommandServiceIntegrationTests()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<AncillaryMappingProfile>();
        });
        _mapper = config.CreateMapper();

        _loggerMock = new Mock<ILogger<SupplierCommandService>>();
        _service = new SupplierCommandService(Context, _mapper, _loggerMock.Object);
    }

    [Fact]
    public async Task CreateAsync_ValidRequest_PersistsToDatabase()
    {
        // Arrange
        var request = TestDataBuilder.CreateSupplierRequest(
            name: "ACME Corporation",
            type: "Vendor");

        // Act
        var result = await _service.CreateAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("ACME Corporation");
        result.Type.Should().Be("Vendor");

        Context.ChangeTracker.Clear();
        var persisted = await Context.Suppliers.FindAsync(result.Id);
        persisted.Should().NotBeNull();
        persisted!.Name.Should().Be("ACME Corporation");
        persisted.Type.Should().Be("Vendor");
        persisted.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task CreateAsync_WithAllFields_PersistsCompleteData()
    {
        // Arrange
        var request = new CreateSupplierRequest
        {
            Name = "Global Supplies Inc",
            Type = "Distributor",
            Description = "International supplier",
            UnitMeasure = "EA",
            Contract = "CONTRACT-2025-001",
            Note = "Preferred supplier"
        };

        // Act
        var result = await _service.CreateAsync(request);

        // Assert
        Context.ChangeTracker.Clear();
        var persisted = await Context.Suppliers.FindAsync(result.Id);
        persisted!.Name.Should().Be("Global Supplies Inc");
        persisted.Type.Should().Be("Distributor");
        persisted.Description.Should().Be("International supplier");
        persisted.UnitMeasure.Should().Be("EA");
        persisted.Contract.Should().Be("CONTRACT-2025-001");
        persisted.Note.Should().Be("Preferred supplier");
    }

    [Fact]
    public async Task CreateAsync_MultipleSuppliers_AllPersisted()
    {
        // Arrange
        var supplier1 = TestDataBuilder.CreateSupplierRequest(name: "Supplier A");
        var supplier2 = TestDataBuilder.CreateSupplierRequest(name: "Supplier B");
        var supplier3 = TestDataBuilder.CreateSupplierRequest(name: "Supplier C");

        // Act
        await _service.CreateAsync(supplier1);
        await _service.CreateAsync(supplier2);
        await _service.CreateAsync(supplier3);

        // Assert
        var suppliers = await Context.Suppliers.ToListAsync();
        suppliers.Should().HaveCount(3);
        suppliers.Should().Contain(s => s.Name == "Supplier A");
        suppliers.Should().Contain(s => s.Name == "Supplier B");
        suppliers.Should().Contain(s => s.Name == "Supplier C");
    }

    [Fact]
    public async Task CreateAsync_DifferentSupplierTypes_AllPersisted()
    {
        // Arrange
        var vendor = TestDataBuilder.CreateSupplierRequest(type: "Vendor");
        var distributor = TestDataBuilder.CreateSupplierRequest(type: "Distributor");
        var manufacturer = TestDataBuilder.CreateSupplierRequest(type: "Manufacturer");

        // Act
        await _service.CreateAsync(vendor);
        await _service.CreateAsync(distributor);
        await _service.CreateAsync(manufacturer);

        // Assert
        var suppliers = await Context.Suppliers.ToListAsync();
        suppliers.Should().HaveCount(3);
        suppliers.Select(s => s.Type).Should().Contain(new[] { "Vendor", "Distributor", "Manufacturer" });
    }

    [Fact]
    public async Task UpdateAsync_ExistingSupplier_PersistsChanges()
    {
        // Arrange
        var createRequest = TestDataBuilder.CreateSupplierRequest(
            name: "Original Name",
            type: "Vendor");
        var created = await _service.CreateAsync(createRequest);

        var updateRequest = new UpdateSupplierRequest
        {
            Name = "Updated Name",
            Type = "Distributor",
            Description = "Updated description",
            UnitMeasure = "KG",
            Contract = "NEW-CONTRACT",
            Note = "Updated note"
        };

        // Act
        var result = await _service.UpdateAsync(created.Id, updateRequest);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Updated Name");
        result.Type.Should().Be("Distributor");

        Context.ChangeTracker.Clear();
        var persisted = await Context.Suppliers.FindAsync(created.Id);
        persisted!.Name.Should().Be("Updated Name");
        persisted.Type.Should().Be("Distributor");
        persisted.UnitMeasure.Should().Be("KG");
        persisted.Contract.Should().Be("NEW-CONTRACT");
        persisted.Note.Should().Be("Updated note");
    }

    [Fact]
    public async Task UpdateAsync_PartialUpdate_OnlyChangesSpecifiedFields()
    {
        // Arrange
        var createRequest = new CreateSupplierRequest
        {
            Name = "Original Supplier",
            Type = "Vendor",
            Description = "Original description",
            Contract = "ORIGINAL-CONTRACT"
        };
        var created = await _service.CreateAsync(createRequest);

        var updateRequest = new UpdateSupplierRequest
        {
            Name = "Updated Supplier",
            Type = "Vendor", // Same
            Description = "Original description", // Same
            Contract = "ORIGINAL-CONTRACT" // Same
        };

        // Act
        await _service.UpdateAsync(created.Id, updateRequest);

        // Assert
        Context.ChangeTracker.Clear();
        var updated = await Context.Suppliers.FindAsync(created.Id);
        updated!.Name.Should().Be("Updated Supplier");
        updated.Type.Should().Be("Vendor");
        updated.Contract.Should().Be("ORIGINAL-CONTRACT");
    }

    [Fact]
    public async Task UpdateAsync_NonExistentSupplier_ReturnsNull()
    {
        // Arrange
        var updateRequest = new UpdateSupplierRequest
        {
            Name = "Test",
            Type = "Vendor"
        };
        var nonExistentId = 99999;

        // Act
        var result = await _service.UpdateAsync(nonExistentId, updateRequest);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task SoftDeleteAsync_ExistingSupplier_SetsIsActiveFalse()
    {
        // Arrange
        var createRequest = TestDataBuilder.CreateSupplierRequest();
        var created = await _service.CreateAsync(createRequest);

        // Act
        var result = await _service.SoftDeleteAsync(created.Id);

        // Assert
        result.Should().BeTrue();

        Context.ChangeTracker.Clear();
        var deleted = await Context.Suppliers.FindAsync(created.Id);
        deleted.Should().NotBeNull();
        deleted!.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task SoftDeleteAsync_UpdatesLastUpdatedDate()
    {
        // Arrange
        var createRequest = TestDataBuilder.CreateSupplierRequest();
        var created = await _service.CreateAsync(createRequest);

        Context.ChangeTracker.Clear();
        var original = await Context.Suppliers.FindAsync(created.Id);
        var originalLastUpdatedDate = original!.LastUpdatedDate;

        await Task.Delay(100);
        var beforeDelete = DateTime.UtcNow;

        // Act
        await _service.SoftDeleteAsync(created.Id);

        // Assert
        Context.ChangeTracker.Clear();
        var deleted = await Context.Suppliers.FindAsync(created.Id);
        deleted!.LastUpdatedDate.Should().BeAfter(originalLastUpdatedDate);
        deleted.LastUpdatedDate.Should().BeCloseTo(beforeDelete, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task SoftDeleteAsync_NonExistentSupplier_ReturnsFalse()
    {
        // Arrange
        var nonExistentId = 99999;

        // Act
        var result = await _service.SoftDeleteAsync(nonExistentId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task HardDeleteAsync_ExistingSupplier_RemovesFromDatabase()
    {
        // Arrange
        var createRequest = TestDataBuilder.CreateSupplierRequest();
        var created = await _service.CreateAsync(createRequest);

        // Act
        var result = await _service.HardDeleteAsync(created.Id);

        // Assert
        result.Should().BeTrue();

        Context.ChangeTracker.Clear();
        var exists = await Context.Suppliers.AnyAsync(s => s.Id == created.Id);
        exists.Should().BeFalse();
    }

    [Fact]
    public async Task HardDeleteAsync_NonExistentSupplier_ReturnsFalse()
    {
        // Arrange
        var nonExistentId = 99999;

        // Act
        var result = await _service.HardDeleteAsync(nonExistentId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task FullCRUDCycle_Supplier_WorksEndToEnd()
    {
        // Create
        var createRequest = TestDataBuilder.CreateSupplierRequest(
            name: "Tech Solutions Ltd",
            type: "Technology Vendor");
        var created = await _service.CreateAsync(createRequest);
        created.Name.Should().Be("Tech Solutions Ltd");
        created.Type.Should().Be("Technology Vendor");

        // Update
        var updateRequest = new UpdateSupplierRequest
        {
            Name = "Tech Solutions Limited",
            Type = "Technology Vendor",
            Description = "Primary technology supplier",
            Contract = "TECH-2025-001"
        };
        var updated = await _service.UpdateAsync(created.Id, updateRequest);
        updated!.Name.Should().Be("Tech Solutions Limited");
        updated.Contract.Should().Be("TECH-2025-001");

        // Soft Delete
        await _service.SoftDeleteAsync(created.Id);
        Context.ChangeTracker.Clear();
        var softDeleted = await Context.Suppliers.FindAsync(created.Id);
        softDeleted!.IsActive.Should().BeFalse();

        // Hard Delete
        await _service.HardDeleteAsync(created.Id);
        Context.ChangeTracker.Clear();
        var hardDeleted = await Context.Suppliers.FindAsync(created.Id);
        hardDeleted.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_SetsAuditFieldsCorrectly()
    {
        // Arrange
        var request = TestDataBuilder.CreateSupplierRequest();
        var beforeCreate = DateTime.UtcNow;

        // Act
        var result = await _service.CreateAsync(request);

        // Assert
        Context.ChangeTracker.Clear();
        var entity = await Context.Suppliers.FindAsync(result.Id);
        entity.Should().NotBeNull();
        entity!.CreatedDate.Should().BeCloseTo(beforeCreate, TimeSpan.FromSeconds(5));
        entity.LastUpdatedDate.Should().BeCloseTo(beforeCreate, TimeSpan.FromSeconds(5));
        entity.CreatedDate.Should().Be(entity.LastUpdatedDate);
        entity.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateAsync_UpdatesLastUpdatedDateOnly()
    {
        // Arrange
        var createRequest = TestDataBuilder.CreateSupplierRequest();
        var created = await _service.CreateAsync(createRequest);

        Context.ChangeTracker.Clear();
        var originalEntity = await Context.Suppliers.FindAsync(created.Id);
        var originalCreatedDate = originalEntity!.CreatedDate;
        var originalLastUpdatedDate = originalEntity.LastUpdatedDate;

        await Task.Delay(100);
        var beforeUpdate = DateTime.UtcNow;

        var updateRequest = new UpdateSupplierRequest
        {
            Name = "Updated Name",
            Type = "Updated Type"
        };

        // Act
        await _service.UpdateAsync(created.Id, updateRequest);

        // Assert
        Context.ChangeTracker.Clear();
        var updated = await Context.Suppliers.FindAsync(created.Id);
        updated!.CreatedDate.Should().Be(originalCreatedDate);
        updated.LastUpdatedDate.Should().BeAfter(originalLastUpdatedDate);
        updated.LastUpdatedDate.Should().BeCloseTo(beforeUpdate, TimeSpan.FromSeconds(5));
    }
}
