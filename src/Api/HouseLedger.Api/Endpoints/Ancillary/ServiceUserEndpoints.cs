using Asp.Versioning.Builder;
using HouseLedger.Services.Ancillary.Application.Contracts.ServiceUsers;
using HouseLedger.Services.Ancillary.Application.Interfaces;

namespace HouseLedger.Api.Endpoints.Ancillary;

/// <summary>
/// ServiceUser endpoints with full CRUD operations.
/// </summary>
public static class ServiceUserEndpoints
{
    public static RouteGroupBuilder MapServiceUserEndpointsV1(this RouteGroupBuilder group)
    {
        // GET /api/v1/serviceusers - Get all
        group.MapGet("/", async (
            IServiceUserQueryService queryService,
            CancellationToken cancellationToken) =>
        {
            var serviceUsers = await queryService.GetAllAsync(false, cancellationToken);
            return Results.Ok(serviceUsers);
        })
        .WithName("GetAllServiceUsers")
        .WithSummary("Get all active service users")
        .WithDescription("Returns all active service users ordered by surname and name");

        // GET /api/v1/serviceusers/{id} - Get by ID
        group.MapGet("/{id:int}", async (
            int id,
            IServiceUserQueryService queryService,
            CancellationToken cancellationToken) =>
        {
            var serviceUser = await queryService.GetByIdAsync(id, cancellationToken);
            return serviceUser is not null
                ? Results.Ok(serviceUser)
                : Results.NotFound(new { Message = $"Service user with ID {id} not found" });
        })
        .WithName("GetServiceUserById")
        .WithSummary("Get service user by ID");

        // POST /api/v1/serviceusers - Create new service user
        group.MapPost("/", async (
            CreateServiceUserRequest request,
            IServiceUserCommandService commandService,
            CancellationToken cancellationToken) =>
        {
            var serviceUser = await commandService.CreateAsync(request, cancellationToken);
            return Results.Created($"/api/v1/serviceusers/{serviceUser.Id}", serviceUser);
        })
        .WithName("CreateServiceUser")
        .WithSummary("Create a new service user");

        // PUT /api/v1/serviceusers/{id} - Update service user
        group.MapPut("/{id:int}", async (
            int id,
            UpdateServiceUserRequest request,
            IServiceUserCommandService commandService,
            CancellationToken cancellationToken) =>
        {
            var serviceUser = await commandService.UpdateAsync(id, request, cancellationToken);
            return serviceUser is not null
                ? Results.Ok(serviceUser)
                : Results.NotFound(new { Message = $"Service user with ID {id} not found" });
        })
        .WithName("UpdateServiceUser")
        .WithSummary("Update an existing service user");

        // DELETE /api/v1/serviceusers/{id}/soft - Soft delete (set IsActive = false)
        group.MapDelete("/{id:int}/soft", async (
            int id,
            IServiceUserCommandService commandService,
            CancellationToken cancellationToken) =>
        {
            var success = await commandService.SoftDeleteAsync(id, cancellationToken);
            return success
                ? Results.NoContent()
                : Results.NotFound(new { Message = $"Service user with ID {id} not found" });
        })
        .WithName("SoftDeleteServiceUser")
        .WithSummary("Soft delete a service user (set IsActive = false)");

        // DELETE /api/v1/serviceusers/{id}/hard - Hard delete (permanent removal)
        group.MapDelete("/{id:int}/hard", async (
            int id,
            IServiceUserCommandService commandService,
            CancellationToken cancellationToken) =>
        {
            var success = await commandService.HardDeleteAsync(id, cancellationToken);
            return success
                ? Results.NoContent()
                : Results.NotFound(new { Message = $"Service user with ID {id} not found" });
        })
        .WithName("HardDeleteServiceUser")
        .WithSummary("Hard delete a service user (permanent removal from database)");

        return group;
    }
}
