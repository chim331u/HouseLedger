using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace HouseLedger.BuildingBlocks.Authentication.Configuration;

/// <summary>
/// Extension methods for configuring JWT authentication.
/// </summary>
public static class JwtAuthenticationExtensions
{
    /// <summary>
    /// Adds JWT Bearer authentication to the application.
    /// Reads configuration from "JwtSettings" section in appsettings.json.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Bind and validate JWT settings
        var jwtSettings = new JwtSettings();
        configuration.GetSection(JwtSettings.SectionName).Bind(jwtSettings);
        jwtSettings.Validate();

        // Log the configuration values for debugging
        var logger = services.BuildServiceProvider().GetService<ILogger<JwtSettings>>();
        if (logger != null)
        {
            logger.LogInformation("JWT Configuration loaded - Issuer: {Issuer}, Audience: {Audience}, ExpirationMinutes: {ExpirationMinutes}",
                jwtSettings.Issuer, jwtSettings.Audience, jwtSettings.TokenExpirationMinutes);
        }

        // Register JwtSettings as options
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));

        // Configure JWT Bearer authentication
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.SaveToken = true;
            options.RequireHttpsMetadata = false; // Allow HTTP for development (set to true in production)
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ClockSkew = TimeSpan.Zero, // No tolerance for expired tokens
                ValidIssuer = jwtSettings.Issuer,
                ValidAudience = jwtSettings.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(jwtSettings.SecretKey))
            };

            // Enhanced event handlers for debugging and logging
            options.Events = new JwtBearerEvents
            {
                OnAuthenticationFailed = context =>
                {
                    var logger = context.HttpContext.RequestServices.GetService<ILogger<JwtBearerEvents>>();

                    if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                    {
                        context.Response.Headers["Token-Expired"] = "true";
                        logger?.LogWarning("JWT token expired for request {Path}", context.Request.Path);
                    }
                    else
                    {
                        logger?.LogError(context.Exception,
                            "JWT authentication failed for request {Path}. Error: {ErrorMessage}",
                            context.Request.Path, context.Exception.Message);
                    }

                    return Task.CompletedTask;
                },
                OnChallenge = context =>
                {
                    var logger = context.HttpContext.RequestServices.GetService<ILogger<JwtBearerEvents>>();
                    logger?.LogWarning("JWT authentication challenge for request {Path}. Error: {Error}, ErrorDescription: {ErrorDescription}",
                        context.Request.Path, context.Error, context.ErrorDescription);
                    return Task.CompletedTask;
                },
                OnTokenValidated = context =>
                {
                    var logger = context.HttpContext.RequestServices.GetService<ILogger<JwtBearerEvents>>();
                    var userId = context.Principal?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                    logger?.LogDebug("JWT token validated successfully for user {UserId} on request {Path}",
                        userId, context.Request.Path);
                    return Task.CompletedTask;
                },
                OnMessageReceived = context =>
                {
                    var logger = context.HttpContext.RequestServices.GetService<ILogger<JwtBearerEvents>>();

                    if (!string.IsNullOrEmpty(context.Token))
                    {
                        // Log token receipt (first 20 chars only for security)
                        var tokenPreview = context.Token.Length > 20 ? context.Token.Substring(0, 20) + "..." : context.Token;
                        logger?.LogDebug("JWT token received for request {Path}: {TokenPreview}",
                            context.Request.Path, tokenPreview);
                    }
                    else
                    {
                        logger?.LogDebug("No JWT token found in request {Path}", context.Request.Path);
                    }

                    return Task.CompletedTask;
                }
            };
        });

        // Add authorization services
        services.AddAuthorization();

        return services;
    }

    /// <summary>
    /// Adds authentication and authorization middleware to the application pipeline.
    /// Call this after UseCors() and before mapping endpoints.
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <returns>The application builder for chaining.</returns>
    public static IApplicationBuilder UseJwtAuthentication(this IApplicationBuilder app)
    {
        app.UseAuthentication();
        app.UseAuthorization();
        return app;
    }
}
