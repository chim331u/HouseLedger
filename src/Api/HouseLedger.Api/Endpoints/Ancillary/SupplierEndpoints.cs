using HouseLedger.Services.Ancillary.Application.Contracts.Suppliers;
using HouseLedger.Services.Ancillary.Application.Interfaces;

namespace HouseLedger.Api.Endpoints.Ancillary;

/// <summary>
/// Endpoints for Supplier with full CRUD operations.
/// </summary>
public static class SupplierEndpoints
{
    public static RouteGroupBuilder MapSupplierEndpointsV1(this RouteGroupBuilder group)
    {
        // GET /api/v1/suppliers - Get all
        group.MapGet("/", async (
            ISupplierQueryService queryService,
            CancellationToken cancellationToken) =>
        {
            var suppliers = await queryService.GetAllAsync(cancellationToken);
            return Results.Ok(suppliers);
        })
        .WithName("GetAllSuppliers")
        .WithSummary("Get all active suppliers");

        // GET /api/v1/suppliers/{id} - Get by ID
        group.MapGet("/{id:int}", async (
            int id,
            ISupplierQueryService queryService,
            CancellationToken cancellationToken) =>
        {
            var supplier = await queryService.GetByIdAsync(id, cancellationToken);
            return supplier is not null
                ? Results.Ok(supplier)
                : Results.NotFound(new { Message = $"Supplier with ID {id} not found" });
        })
        .WithName("GetSupplierById")
        .WithSummary("Get supplier by ID");

        // GET /api/v1/suppliers/type/{type} - Get by type
        group.MapGet("/type/{type}", async (
            string type,
            ISupplierQueryService queryService,
            CancellationToken cancellationToken) =>
        {
            var suppliers = await queryService.GetByTypeAsync(type, cancellationToken);
            return Results.Ok(suppliers);
        })
        .WithName("GetSuppliersByType")
        .WithSummary("Get suppliers by type");

        // POST /api/v1/suppliers - Create new supplier
        group.MapPost("/", async (
            CreateSupplierRequest request,
            ISupplierCommandService commandService,
            CancellationToken cancellationToken) =>
        {
            var supplier = await commandService.CreateAsync(request, cancellationToken);
            return Results.Created($"/api/v1/suppliers/{supplier.Id}", supplier);
        })
        .WithName("CreateSupplier")
        .WithSummary("Create a new supplier");

        // PUT /api/v1/suppliers/{id} - Update supplier
        group.MapPut("/{id:int}", async (
            int id,
            UpdateSupplierRequest request,
            ISupplierCommandService commandService,
            CancellationToken cancellationToken) =>
        {
            var supplier = await commandService.UpdateAsync(id, request, cancellationToken);
            return supplier is not null
                ? Results.Ok(supplier)
                : Results.NotFound(new { Message = $"Supplier with ID {id} not found" });
        })
        .WithName("UpdateSupplier")
        .WithSummary("Update an existing supplier");

        // DELETE /api/v1/suppliers/{id}/soft - Soft delete
        group.MapDelete("/{id:int}/soft", async (
            int id,
            ISupplierCommandService commandService,
            CancellationToken cancellationToken) =>
        {
            var success = await commandService.SoftDeleteAsync(id, cancellationToken);
            return success
                ? Results.NoContent()
                : Results.NotFound(new { Message = $"Supplier with ID {id} not found" });
        })
        .WithName("SoftDeleteSupplier")
        .WithSummary("Soft delete a supplier (set IsActive = false)");

        // DELETE /api/v1/suppliers/{id}/hard - Hard delete
        group.MapDelete("/{id:int}/hard", async (
            int id,
            ISupplierCommandService commandService,
            CancellationToken cancellationToken) =>
        {
            var success = await commandService.HardDeleteAsync(id, cancellationToken);
            return success
                ? Results.NoContent()
                : Results.NotFound(new { Message = $"Supplier with ID {id} not found" });
        })
        .WithName("HardDeleteSupplier")
        .WithSummary("Hard delete a supplier (permanent removal from database)");

        return group;
    }
}
