using Asp.Versioning.Builder;
using HouseLedger.Services.HouseThings.Application.Contracts.HouseThings;
using HouseLedger.Services.HouseThings.Application.Interfaces;

namespace HouseLedger.Api.Endpoints.HouseThings;

/// <summary>
/// HouseThing endpoints using Minimal APIs with Traditional Services (simple CRUD).
/// GET /api/v1/housethings/{id} - Get house thing by ID
/// GET /api/v1/housethings - Get all house things
/// GET /api/v1/housethings/room/{roomId} - Get house things by room
/// GET /api/v1/housethings/history/{historyId} - Get house things by history
/// POST /api/v1/housethings - Create new house thing
/// POST /api/v1/housethings/{id}/renew - Renew house thing (soft delete old, create new with same history)
/// PUT /api/v1/housethings/{id} - Update house thing
/// DELETE /api/v1/housethings/{id}/soft - Soft delete house thing
/// DELETE /api/v1/housethings/{id}/hard - Hard delete house thing
/// </summary>
public static class HouseThingEndpoints
{
    public static RouteGroupBuilder MapHouseThingEndpointsV1(this RouteGroupBuilder group)
    {
        // GET /api/v1/housethings/{id} - Get by ID
        group.MapGet("/{id:int}", async (
            int id,
            IHouseThingQueryService queryService,
            CancellationToken cancellationToken) =>
        {
            var houseThing = await queryService.GetByIdAsync(id, cancellationToken);
            return houseThing is not null
                ? Results.Ok(houseThing)
                : Results.NotFound(new { Message = $"House thing with ID {id} not found" });
        })
        .WithName("GetHouseThingById")
        .WithSummary("Get house thing by ID")
        .Produces<HouseThingDto>()
        .ProducesProblem(StatusCodes.Status404NotFound);

        // GET /api/v1/housethings - Get all
        group.MapGet("/", async (
            IHouseThingQueryService queryService,
            CancellationToken cancellationToken) =>
        {
            var houseThings = await queryService.GetAllAsync(cancellationToken);
            return Results.Ok(houseThings);
        })
        .WithName("GetAllHouseThings")
        .WithSummary("Get all house things")
        .Produces<IEnumerable<HouseThingDto>>();

        // GET /api/v1/housethings/room/{roomId} - Get by room
        group.MapGet("/room/{roomId:int}", async (
            int roomId,
            IHouseThingQueryService queryService,
            CancellationToken cancellationToken) =>
        {
            var houseThings = await queryService.GetByRoomIdAsync(roomId, cancellationToken);
            return Results.Ok(houseThings);
        })
        .WithName("GetHouseThingsByRoom")
        .WithSummary("Get all house things in a specific room")
        .Produces<IEnumerable<HouseThingDto>>();

        // GET /api/v1/housethings/history/{historyId} - Get history
        group.MapGet("/history/{historyId:int}", async (
            int historyId,
            IHouseThingQueryService queryService,
            CancellationToken cancellationToken) =>
        {
            var houseThings = await queryService.GetHistoryAsync(historyId, cancellationToken);
            return Results.Ok(houseThings);
        })
        .WithName("GetHouseThingHistory")
        .WithSummary("Get all versions of a house thing by history ID")
        .Produces<IEnumerable<HouseThingDto>>();

        // POST /api/v1/housethings - Create new house thing
        group.MapPost("/", async (
            CreateHouseThingRequest request,
            IHouseThingCommandService commandService,
            CancellationToken cancellationToken) =>
        {
            var houseThing = await commandService.CreateAsync(request, cancellationToken);
            return Results.Created($"/api/v1/housethings/{houseThing.Id}", houseThing);
        })
        .WithName("CreateHouseThing")
        .WithSummary("Create a new house thing")
        .Produces<HouseThingDto>(StatusCodes.Status201Created);

        // POST /api/v1/housethings/{id}/renew - Renew house thing
        group.MapPost("/{id:int}/renew", async (
            int id,
            CreateHouseThingRequest request,
            IHouseThingCommandService commandService,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var houseThing = await commandService.RenewAsync(id, request, cancellationToken);
                return Results.Created($"/api/v1/housethings/{houseThing.Id}", houseThing);
            }
            catch (InvalidOperationException ex)
            {
                return Results.NotFound(new { Message = ex.Message });
            }
        })
        .WithName("RenewHouseThing")
        .WithSummary("Renew a house thing (creates new version with same history, soft deletes old)")
        .Produces<HouseThingDto>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status404NotFound);

        // PUT /api/v1/housethings/{id} - Update house thing
        group.MapPut("/{id:int}", async (
            int id,
            UpdateHouseThingRequest request,
            IHouseThingCommandService commandService,
            CancellationToken cancellationToken) =>
        {
            var houseThing = await commandService.UpdateAsync(id, request, cancellationToken);
            return houseThing is not null
                ? Results.Ok(houseThing)
                : Results.NotFound(new { Message = $"House thing with ID {id} not found" });
        })
        .WithName("UpdateHouseThing")
        .WithSummary("Update an existing house thing")
        .Produces<HouseThingDto>()
        .ProducesProblem(StatusCodes.Status404NotFound);

        // DELETE /api/v1/housethings/{id}/soft - Soft delete
        group.MapDelete("/{id:int}/soft", async (
            int id,
            IHouseThingCommandService commandService,
            CancellationToken cancellationToken) =>
        {
            var success = await commandService.SoftDeleteAsync(id, cancellationToken);
            return success
                ? Results.NoContent()
                : Results.NotFound(new { Message = $"House thing with ID {id} not found" });
        })
        .WithName("SoftDeleteHouseThing")
        .WithSummary("Soft delete a house thing (set IsActive = false)")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status404NotFound);

        // DELETE /api/v1/housethings/{id}/hard - Hard delete
        group.MapDelete("/{id:int}/hard", async (
            int id,
            IHouseThingCommandService commandService,
            CancellationToken cancellationToken) =>
        {
            var success = await commandService.HardDeleteAsync(id, cancellationToken);
            return success
                ? Results.NoContent()
                : Results.NotFound(new { Message = $"House thing with ID {id} not found" });
        })
        .WithName("HardDeleteHouseThing")
        .WithSummary("Hard delete a house thing (permanent removal from database)")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status404NotFound);

        return group;
    }
}
