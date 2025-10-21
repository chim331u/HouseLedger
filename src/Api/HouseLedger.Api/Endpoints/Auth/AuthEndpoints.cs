using HouseLedger.Api.Services.Auth;
using HouseLedger.BuildingBlocks.Authentication.Contract;
using HouseLedger.BuildingBlocks.Authentication.Models;
using Microsoft.AspNetCore.Mvc;

namespace HouseLedger.Api.Endpoints.Auth;

/// <summary>
/// Authentication endpoints for user login and registration.
/// </summary>
public static class AuthEndpoints
{
    /// <summary>
    /// Maps authentication endpoints to the application.
    /// </summary>
    public static RouteGroupBuilder MapAuthEndpointsV1(this RouteGroupBuilder group)
    {
        // POST /api/v1/auth/login
        group.MapPost("/login", LoginAsync)
            .WithName("Login")
            .WithSummary("Authenticate user and obtain JWT token")
            .WithDescription("Authenticates a user with email and password, returning a JWT bearer token on success.")
            .Produces<AuthResponse>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
            .AllowAnonymous();

        // POST /api/v1/auth/register
        group.MapPost("/register", RegisterAsync)
            .WithName("Register")
            .WithSummary("Register a new user")
            .WithDescription("Creates a new user account with the provided credentials.")
            .Produces<RegistrationRequest>(StatusCodes.Status201Created)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .AllowAnonymous();

        // GET /api/v1/auth/test (protected endpoint for testing)
        group.MapGet("/test", TestAuthAsync)
            .WithName("TestAuth")
            .WithSummary("Test authentication (requires valid JWT token)")
            .WithDescription("Returns user information if authenticated. Use this to test JWT token validation.")
            .Produces<object>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
            .RequireAuthorization();

        return group;
    }

    /// <summary>
    /// Login endpoint handler.
    /// </summary>
    private static async Task<IResult> LoginAsync(
        [FromBody] AuthRequest request,
        [FromServices] IAuthService authService,
        [FromServices] ILogger<AuthRequest> logger)
    {
        // Validate model state
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            logger.LogWarning("Login attempt with invalid model state");
            return Results.BadRequest(new ProblemDetails
            {
                Title = "Invalid request",
                Detail = "Email and password are required",
                Status = StatusCodes.Status400BadRequest
            });
        }

        // Attempt login
        var response = await authService.LoginAsync(request);

        if (response == null)
        {
            logger.LogWarning("Login failed for email: {Email}", request.Email);
            return Results.Unauthorized();
        }

        logger.LogInformation("Login successful for email: {Email}", request.Email);
        return Results.Ok(response);
    }

    /// <summary>
    /// Register endpoint handler.
    /// </summary>
    private static async Task<IResult> RegisterAsync(
        [FromBody] RegistrationRequest request,
        [FromServices] IAuthService authService,
        [FromServices] ILogger<RegistrationRequest> logger)
    {
        // Validate model state
        if (string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.Username) ||
            string.IsNullOrWhiteSpace(request.Password))
        {
            logger.LogWarning("Registration attempt with invalid model state");
            return Results.BadRequest(new ProblemDetails
            {
                Title = "Invalid request",
                Detail = "Email, username, and password are required",
                Status = StatusCodes.Status400BadRequest
            });
        }

        if (request.Password != request.ConfirmPassword)
        {
            logger.LogWarning("Registration attempt with mismatched passwords");
            return Results.BadRequest(new ProblemDetails
            {
                Title = "Invalid request",
                Detail = "Passwords do not match",
                Status = StatusCodes.Status400BadRequest
            });
        }

        // Attempt registration
        var (success, errors) = await authService.RegisterAsync(request);

        if (!success)
        {
            logger.LogWarning("Registration failed for email: {Email}. Errors: {Errors}",
                request.Email, string.Join(", ", errors));

            return Results.BadRequest(new ProblemDetails
            {
                Title = "Registration failed",
                Detail = string.Join(", ", errors),
                Status = StatusCodes.Status400BadRequest
            });
        }

        logger.LogInformation("Registration successful for email: {Email}", request.Email);

        // Clear password from response for security
        var responseRequest = new RegistrationRequest
        {
            Email = request.Email,
            Username = request.Username,
            Password = string.Empty,
            ConfirmPassword = string.Empty
        };

        return Results.Created($"/api/v1/auth/users/{request.Email}", responseRequest);
    }

    /// <summary>
    /// Test authentication endpoint handler.
    /// Returns user information if authenticated.
    /// </summary>
    private static IResult TestAuthAsync(HttpContext context)
    {
        var user = context.User;

        if (user.Identity?.IsAuthenticated != true)
        {
            return Results.Unauthorized();
        }

        var userInfo = new
        {
            IsAuthenticated = user.Identity.IsAuthenticated,
            AuthenticationType = user.Identity.AuthenticationType,
            Name = user.Identity.Name,
            Claims = user.Claims.Select(c => new { c.Type, c.Value }).ToList()
        };

        return Results.Ok(userInfo);
    }
}
