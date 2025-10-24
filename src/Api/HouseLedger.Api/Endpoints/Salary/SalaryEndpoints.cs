using HouseLedger.Services.Salary.Application.Contracts.Salaries;
using HouseLedger.Services.Salary.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HouseLedger.Api.Endpoints.Salary;

/// <summary>
/// Salary endpoints for managing salary entries.
/// </summary>
public static class SalaryEndpoints
{
    public static RouteGroupBuilder MapSalaryEndpointsV1(this RouteGroupBuilder group)
    {
        // GET /api/v1/salaries
        group.MapGet("/", GetAllSalaries)
            .WithName("GetAllSalaries")
            .WithSummary("Get all active salaries")
            .WithDescription("Returns all active salary entries ordered by salary date (most recent first)")
            .Produces<IEnumerable<SalaryDto>>(StatusCodes.Status200OK);
            //.RequireAuthorization();

        // GET /api/v1/salaries/{id}
        group.MapGet("/{id:int}", GetSalaryById)
            .WithName("GetSalaryById")
            .WithSummary("Get salary by ID")
            .WithDescription("Returns a single salary entry by its ID")
            .Produces<SalaryDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);
            //.RequireAuthorization();

        // GET /api/v1/salaries/user/{userId}
        group.MapGet("/user/{userId:int}", GetSalariesByUserId)
            .WithName("GetSalariesByUserId")
            .WithSummary("Get salaries by user ID")
            .WithDescription("Returns all active salary entries for a specific user")
            .Produces<IEnumerable<SalaryDto>>(StatusCodes.Status200OK);
            //.RequireAuthorization();

        // GET /api/v1/salaries/year/{year}
        group.MapGet("/year/{year}", GetSalariesByYear)
            .WithName("GetSalariesByYear")
            .WithSummary("Get salaries by year")
            .WithDescription("Returns all active salary entries for a specific year")
            .Produces<IEnumerable<SalaryDto>>(StatusCodes.Status200OK);
            //.RequireAuthorization();

        // POST /api/v1/salaries
        group.MapPost("/", CreateSalary)
            .WithName("CreateSalary")
            .WithSummary("Create a new salary entry")
            .WithDescription("Creates a new salary entry and returns the created entity")
            .Produces<SalaryDto>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest);
            //.RequireAuthorization();

        // PUT /api/v1/salaries/{id}
        group.MapPut("/{id:int}", UpdateSalary)
            .WithName("UpdateSalary")
            .WithSummary("Update an existing salary entry")
            .WithDescription("Updates an existing salary entry and returns the updated entity")
            .Produces<SalaryDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status400BadRequest);
            //.RequireAuthorization();

        // DELETE /api/v1/salaries/{id}
        group.MapDelete("/{id:int}", DeleteSalary)
            .WithName("DeleteSalary")
            .WithSummary("Soft delete a salary entry")
            .WithDescription("Soft deletes a salary entry by setting IsActive to false")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);
            //.RequireAuthorization();

        return group;
    }

    private static async Task<IResult> GetAllSalaries(
        [FromServices] ISalaryQueryService queryService,
        [FromQuery] bool includeInactive = false)
    {
        var result = await queryService.GetAllAsync(includeInactive);
        return Results.Ok(result);
    }

    private static async Task<IResult> GetSalaryById(
        [FromServices] ISalaryQueryService queryService,
        int id)
    {
        var result = await queryService.GetByIdAsync(id);

        if (result == null)
        {
            return Results.NotFound(new { Message = $"Salary with ID {id} not found" });
        }

        return Results.Ok(result);
    }

    private static async Task<IResult> GetSalariesByUserId(
        [FromServices] ISalaryQueryService queryService,
        int userId)
    {
        var result = await queryService.GetByUserIdAsync(userId);
        return Results.Ok(result);
    }

    private static async Task<IResult> GetSalariesByYear(
        [FromServices] ISalaryQueryService queryService,
        string year)
    {
        var result = await queryService.GetByYearAsync(year);
        return Results.Ok(result);
    }

    private static async Task<IResult> CreateSalary(
        [FromServices] ISalaryCommandService commandService,
        [FromBody] CreateSalaryRequest request)
    {
        var result = await commandService.CreateAsync(request);
        return Results.Created($"/api/v1/salaries/{result.Id}", result);
    }

    private static async Task<IResult> UpdateSalary(
        [FromServices] ISalaryCommandService commandService,
        int id,
        [FromBody] UpdateSalaryRequest request)
    {
        if (id != request.Id)
        {
            return Results.BadRequest(new { Message = "ID in URL does not match ID in request body" });
        }

        var result = await commandService.UpdateAsync(id, request);

        if (result == null)
        {
            return Results.NotFound(new { Message = $"Salary with ID {id} not found" });
        }

        return Results.Ok(result);
    }

    private static async Task<IResult> DeleteSalary(
        [FromServices] ISalaryCommandService commandService,
        int id)
    {
        var result = await commandService.SoftDeleteAsync(id);

        if (!result)
        {
            return Results.NotFound(new { Message = $"Salary with ID {id} not found" });
        }

        return Results.NoContent();
    }
}
