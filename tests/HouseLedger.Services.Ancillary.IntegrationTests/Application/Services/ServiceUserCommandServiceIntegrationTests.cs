using AutoMapper;
using HouseLedger.Services.Ancillary.Application.Contracts.ServiceUsers;
using HouseLedger.Services.Ancillary.Application.Mapping;
using HouseLedger.Services.Ancillary.Application.Services;
using HouseLedger.Services.Ancillary.IntegrationTests.Fixtures;
using HouseLedger.Services.Ancillary.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HouseLedger.Services.Ancillary.IntegrationTests.Application.Services;

/// <summary>
/// Integration tests for ServiceUserCommandService with real database operations.
/// Tests full CRUD lifecycle, transaction handling, and database constraints.
/// </summary>
public class ServiceUserCommandServiceIntegrationTests : IntegrationTestBase
{
    private readonly IMapper _mapper;
    private readonly ServiceUserCommandService _service;
    private readonly Mock<ILogger<ServiceUserCommandService>> _loggerMock;

    public ServiceUserCommandServiceIntegrationTests()
    {
        // Setup AutoMapper
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<AncillaryMappingProfile>();
        });
        _mapper = config.CreateMapper();

        // Setup logger mock
        _loggerMock = new Mock<ILogger<ServiceUserCommandService>>();

        // Create service
        _service = new ServiceUserCommandService(Context, _mapper, _loggerMock.Object);
    }

    #region CreateAsync Integration Tests

    [Fact]
    public async Task CreateAsync_ValidRequest_PersistsToDatabase()
    {
        // Arrange
        var request = TestDataBuilder.CreateServiceUserRequest(
            name: "John",
            surname: "Doe",
            note: "Test user");

        // Act
        var result = await _service.CreateAsync(request);

        // Assert - Verify DTO
        result.Should().NotBeNull();
        result.Name.Should().Be("John");
        result.Surname.Should().Be("Doe");
        result.Note.Should().Be("Test user");

        // Assert - Verify database persistence by querying fresh context
        Context.ChangeTracker.Clear();
        var persisted = await Context.ServiceUsers.FindAsync(result.Id);
        persisted.Should().NotBeNull();
        persisted!.Name.Should().Be("John");
        persisted.Surname.Should().Be("Doe");
        persisted.Note.Should().Be("Test user");
        persisted.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task CreateAsync_ValidRequest_SetsAuditFieldsCorrectly()
    {
        // Arrange
        var request = TestDataBuilder.CreateServiceUserRequest();
        var beforeCreate = DateTime.UtcNow;

        // Act
        var result = await _service.CreateAsync(request);

        // Assert
        Context.ChangeTracker.Clear();
        var entity = await Context.ServiceUsers.FindAsync(result.Id);
        entity.Should().NotBeNull();
        entity!.CreatedDate.Should().BeCloseTo(beforeCreate, TimeSpan.FromSeconds(5));
        entity.LastUpdatedDate.Should().BeCloseTo(beforeCreate, TimeSpan.FromSeconds(5));
        entity.CreatedDate.Should().Be(entity.LastUpdatedDate);
        entity.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task CreateAsync_MultipleServiceUsers_AllPersisted()
    {
        // Arrange
        var request1 = TestDataBuilder.CreateServiceUserRequest(name: "Alice", surname: "Smith");
        var request2 = TestDataBuilder.CreateServiceUserRequest(name: "Bob", surname: "Jones");
        var request3 = TestDataBuilder.CreateServiceUserRequest(name: "Charlie", surname: "Brown");

        // Act
        var result1 = await _service.CreateAsync(request1);
        var result2 = await _service.CreateAsync(request2);
        var result3 = await _service.CreateAsync(request3);

        // Assert
        var serviceUsers = await Context.ServiceUsers.ToListAsync();
        serviceUsers.Should().HaveCount(3);
        serviceUsers.Should().Contain(su => su.Name == "Alice" && su.Surname == "Smith");
        serviceUsers.Should().Contain(su => su.Name == "Bob" && su.Surname == "Jones");
        serviceUsers.Should().Contain(su => su.Name == "Charlie" && su.Surname == "Brown");
    }

    [Fact]
    public async Task CreateAsync_WithNullValues_HandlesCorrectly()
    {
        // Arrange
        var request = new CreateServiceUserRequest
        {
            Name = null,
            Surname = null,
            Note = null
        };

        // Act
        var result = await _service.CreateAsync(request);

        // Assert
        Context.ChangeTracker.Clear();
        var persisted = await Context.ServiceUsers.FindAsync(result.Id);
        persisted.Should().NotBeNull();
        persisted!.Name.Should().BeNull();
        persisted.Surname.Should().BeNull();
        persisted.Note.Should().BeNull();
    }

    #endregion

    #region UpdateAsync Integration Tests

    [Fact]
    public async Task UpdateAsync_ExistingEntity_PersistsChanges()
    {
        // Arrange
        var createRequest = TestDataBuilder.CreateServiceUserRequest(name: "Original", surname: "Name");
        var created = await _service.CreateAsync(createRequest);

        var updateRequest = TestDataBuilder.UpdateServiceUserRequest(
            name: "Updated",
            surname: "Surname",
            note: "Updated note");

        // Act
        var result = await _service.UpdateAsync(created.Id, updateRequest);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Updated");
        result.Surname.Should().Be("Surname");

        // Verify persistence
        Context.ChangeTracker.Clear();
        var persisted = await Context.ServiceUsers.FindAsync(created.Id);
        persisted!.Name.Should().Be("Updated");
        persisted.Surname.Should().Be("Surname");
        persisted.Note.Should().Be("Updated note");
    }

    [Fact]
    public async Task UpdateAsync_ExistingEntity_UpdatesLastUpdatedDate()
    {
        // Arrange
        var createRequest = TestDataBuilder.CreateServiceUserRequest();
        var created = await _service.CreateAsync(createRequest);

        Context.ChangeTracker.Clear();
        var originalEntity = await Context.ServiceUsers.FindAsync(created.Id);
        var originalCreatedDate = originalEntity!.CreatedDate;
        var originalLastUpdatedDate = originalEntity.LastUpdatedDate;

        await Task.Delay(100); // Ensure time passes
        var beforeUpdate = DateTime.UtcNow;

        var updateRequest = TestDataBuilder.UpdateServiceUserRequest(name: "New Name");

        // Act
        await _service.UpdateAsync(created.Id, updateRequest);

        // Assert
        Context.ChangeTracker.Clear();
        var updated = await Context.ServiceUsers.FindAsync(created.Id);
        updated!.CreatedDate.Should().Be(originalCreatedDate);
        updated.LastUpdatedDate.Should().BeAfter(originalLastUpdatedDate);
        updated.LastUpdatedDate.Should().BeCloseTo(beforeUpdate, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task UpdateAsync_NonExistentEntity_ReturnsNull()
    {
        // Arrange
        var updateRequest = TestDataBuilder.UpdateServiceUserRequest();
        var nonExistentId = 99999;

        // Act
        var result = await _service.UpdateAsync(nonExistentId, updateRequest);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateAsync_PartialUpdate_OnlyChangesSpecifiedFields()
    {
        // Arrange
        var createRequest = TestDataBuilder.CreateServiceUserRequest(
            name: "Original",
            surname: "Surname",
            note: "Original note");
        var created = await _service.CreateAsync(createRequest);

        var updateRequest = TestDataBuilder.UpdateServiceUserRequest(
            name: "Updated Name Only",
            surname: "Surname", // Keep same
            note: "Original note"); // Keep same

        // Act
        await _service.UpdateAsync(created.Id, updateRequest);

        // Assert
        Context.ChangeTracker.Clear();
        var updated = await Context.ServiceUsers.FindAsync(created.Id);
        updated!.Name.Should().Be("Updated Name Only");
        updated.Surname.Should().Be("Surname");
        updated.Note.Should().Be("Original note");
    }

    #endregion

    #region SoftDeleteAsync Integration Tests

    [Fact]
    public async Task SoftDeleteAsync_ExistingEntity_SetsIsActiveFalseInDatabase()
    {
        // Arrange
        var createRequest = TestDataBuilder.CreateServiceUserRequest();
        var created = await _service.CreateAsync(createRequest);

        // Act
        var result = await _service.SoftDeleteAsync(created.Id);

        // Assert
        result.Should().BeTrue();

        Context.ChangeTracker.Clear();
        var deleted = await Context.ServiceUsers.FindAsync(created.Id);
        deleted.Should().NotBeNull();
        deleted!.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task SoftDeleteAsync_ExistingEntity_UpdatesLastUpdatedDate()
    {
        // Arrange
        var createRequest = TestDataBuilder.CreateServiceUserRequest();
        var created = await _service.CreateAsync(createRequest);

        Context.ChangeTracker.Clear();
        var original = await Context.ServiceUsers.FindAsync(created.Id);
        var originalLastUpdatedDate = original!.LastUpdatedDate;

        await Task.Delay(100);
        var beforeDelete = DateTime.UtcNow;

        // Act
        await _service.SoftDeleteAsync(created.Id);

        // Assert
        Context.ChangeTracker.Clear();
        var deleted = await Context.ServiceUsers.FindAsync(created.Id);
        deleted!.LastUpdatedDate.Should().BeAfter(originalLastUpdatedDate);
        deleted.LastUpdatedDate.Should().BeCloseTo(beforeDelete, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task SoftDeleteAsync_NonExistentEntity_ReturnsFalse()
    {
        // Arrange
        var nonExistentId = 99999;

        // Act
        var result = await _service.SoftDeleteAsync(nonExistentId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task SoftDeleteAsync_AlreadyDeleted_CanBeDeletedAgain()
    {
        // Arrange
        var createRequest = TestDataBuilder.CreateServiceUserRequest();
        var created = await _service.CreateAsync(createRequest);
        await _service.SoftDeleteAsync(created.Id);

        // Act
        var result = await _service.SoftDeleteAsync(created.Id);

        // Assert
        result.Should().BeTrue();

        Context.ChangeTracker.Clear();
        var deleted = await Context.ServiceUsers.FindAsync(created.Id);
        deleted!.IsActive.Should().BeFalse();
    }

    #endregion

    #region HardDeleteAsync Integration Tests

    [Fact]
    public async Task HardDeleteAsync_ExistingEntity_RemovesFromDatabase()
    {
        // Arrange
        var createRequest = TestDataBuilder.CreateServiceUserRequest();
        var created = await _service.CreateAsync(createRequest);
        var createdId = created.Id;

        // Act
        var result = await _service.HardDeleteAsync(createdId);

        // Assert
        result.Should().BeTrue();

        Context.ChangeTracker.Clear();
        var exists = await Context.ServiceUsers.AnyAsync(su => su.Id == createdId);
        exists.Should().BeFalse();
    }

    [Fact]
    public async Task HardDeleteAsync_NonExistentEntity_ReturnsFalse()
    {
        // Arrange
        var nonExistentId = 99999;

        // Act
        var result = await _service.HardDeleteAsync(nonExistentId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task HardDeleteAsync_AfterSoftDelete_StillRemovesFromDatabase()
    {
        // Arrange
        var createRequest = TestDataBuilder.CreateServiceUserRequest();
        var created = await _service.CreateAsync(createRequest);
        await _service.SoftDeleteAsync(created.Id);

        // Act
        var result = await _service.HardDeleteAsync(created.Id);

        // Assert
        result.Should().BeTrue();

        Context.ChangeTracker.Clear();
        var exists = await Context.ServiceUsers.AnyAsync(su => su.Id == created.Id);
        exists.Should().BeFalse();
    }

    #endregion

    #region Transaction and Full CRUD Cycle Tests

    [Fact]
    public async Task FullCRUDCycle_WorksEndToEnd()
    {
        // Create
        var createRequest = TestDataBuilder.CreateServiceUserRequest(name: "Test", surname: "User");
        var created = await _service.CreateAsync(createRequest);
        created.Name.Should().Be("Test");
        created.Surname.Should().Be("User");

        // Read
        Context.ChangeTracker.Clear();
        var read = await Context.ServiceUsers.FindAsync(created.Id);
        read.Should().NotBeNull();
        read!.Name.Should().Be("Test");
        read.Surname.Should().Be("User");

        // Update
        var updateRequest = TestDataBuilder.UpdateServiceUserRequest(name: "Updated", surname: "User");
        var updated = await _service.UpdateAsync(created.Id, updateRequest);
        updated!.Name.Should().Be("Updated");

        // Soft Delete
        var softDeleted = await _service.SoftDeleteAsync(created.Id);
        softDeleted.Should().BeTrue();
        Context.ChangeTracker.Clear();
        var afterSoftDelete = await Context.ServiceUsers.FindAsync(created.Id);
        afterSoftDelete!.IsActive.Should().BeFalse();

        // Hard Delete
        var hardDeleted = await _service.HardDeleteAsync(created.Id);
        hardDeleted.Should().BeTrue();
        Context.ChangeTracker.Clear();
        var afterHardDelete = await Context.ServiceUsers.FindAsync(created.Id);
        afterHardDelete.Should().BeNull();
    }

    #endregion
}
