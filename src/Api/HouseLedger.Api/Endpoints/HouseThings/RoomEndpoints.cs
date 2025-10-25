using Asp.Versioning.Builder;
using HouseLedger.Services.HouseThings.Application.Contracts.Rooms;
using HouseLedger.Services.HouseThings.Application.Interfaces;

namespace HouseLedger.Api.Endpoints.HouseThings;

/// <summary>
/// Room endpoints using Minimal APIs with Traditional Services (simple CRUD).
/// GET /api/v1/rooms/{id} - Get room by ID
/// GET /api/v1/rooms - Get all rooms
/// POST /api/v1/rooms - Create new room
/// PUT /api/v1/rooms/{id} - Update room
/// DELETE /api/v1/rooms/{id}/soft - Soft delete room
/// DELETE /api/v1/rooms/{id}/hard - Hard delete room
/// </summary>
public static class RoomEndpoints
{
    public static RouteGroupBuilder MapRoomEndpointsV1(this RouteGroupBuilder group)
    {
        // GET /api/v1/rooms/{id} - Get by ID
        group.MapGet("/{id:int}", async (
            int id,
            IRoomQueryService queryService,
            CancellationToken cancellationToken) =>
        {
            var room = await queryService.GetByIdAsync(id, cancellationToken);
            return room is not null
                ? Results.Ok(room)
                : Results.NotFound(new { Message = $"Room with ID {id} not found" });
        })
        .WithName("GetRoomById")
        .WithSummary("Get room by ID")
        .Produces<RoomDto>()
        .ProducesProblem(StatusCodes.Status404NotFound);

        // GET /api/v1/rooms - Get all
        group.MapGet("/", async (
            IRoomQueryService queryService,
            CancellationToken cancellationToken) =>
        {
            var rooms = await queryService.GetAllAsync(cancellationToken);
            return Results.Ok(rooms);
        })
        .WithName("GetAllRooms")
        .WithSummary("Get all rooms")
        .Produces<IEnumerable<RoomDto>>();

        // POST /api/v1/rooms - Create new room
        group.MapPost("/", async (
            CreateRoomRequest request,
            IRoomCommandService commandService,
            CancellationToken cancellationToken) =>
        {
            var room = await commandService.CreateAsync(request, cancellationToken);
            return Results.Created($"/api/v1/rooms/{room.Id}", room);
        })
        .WithName("CreateRoom")
        .WithSummary("Create a new room")
        .Produces<RoomDto>(StatusCodes.Status201Created);

        // PUT /api/v1/rooms/{id} - Update room
        group.MapPut("/{id:int}", async (
            int id,
            UpdateRoomRequest request,
            IRoomCommandService commandService,
            CancellationToken cancellationToken) =>
        {
            var room = await commandService.UpdateAsync(id, request, cancellationToken);
            return room is not null
                ? Results.Ok(room)
                : Results.NotFound(new { Message = $"Room with ID {id} not found" });
        })
        .WithName("UpdateRoom")
        .WithSummary("Update an existing room")
        .Produces<RoomDto>()
        .ProducesProblem(StatusCodes.Status404NotFound);

        // DELETE /api/v1/rooms/{id}/soft - Soft delete
        group.MapDelete("/{id:int}/soft", async (
            int id,
            IRoomCommandService commandService,
            CancellationToken cancellationToken) =>
        {
            var success = await commandService.SoftDeleteAsync(id, cancellationToken);
            return success
                ? Results.NoContent()
                : Results.NotFound(new { Message = $"Room with ID {id} not found" });
        })
        .WithName("SoftDeleteRoom")
        .WithSummary("Soft delete a room (set IsActive = false)")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status404NotFound);

        // DELETE /api/v1/rooms/{id}/hard - Hard delete
        group.MapDelete("/{id:int}/hard", async (
            int id,
            IRoomCommandService commandService,
            CancellationToken cancellationToken) =>
        {
            var success = await commandService.HardDeleteAsync(id, cancellationToken);
            return success
                ? Results.NoContent()
                : Results.NotFound(new { Message = $"Room with ID {id} not found" });
        })
        .WithName("HardDeleteRoom")
        .WithSummary("Hard delete a room (permanent removal from database)")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status404NotFound);

        return group;
    }
}
