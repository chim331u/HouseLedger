using HouseLedger.Api.Services.Auth;
using HouseLedger.BuildingBlocks.Authentication.Configuration;
using HouseLedger.BuildingBlocks.Authentication.Contract;
using HouseLedger.BuildingBlocks.Authentication.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HouseLedger.BuildingBlocks.Authentication.Services;

/// <summary>
/// Authentication service implementation.
/// Handles user login and registration using ASP.NET Core Identity and JWT tokens.
/// </summary>
public class AuthService : IAuthService
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly AppIdentityDbContext _identityContext;
    private readonly ITokenService _tokenService;
    private readonly JwtSettings _jwtSettings;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        UserManager<IdentityUser> userManager,
        AppIdentityDbContext identityContext,
        ITokenService tokenService,
        IOptions<JwtSettings> jwtSettings,
        ILogger<AuthService> logger)
    {
        _userManager = userManager;
        _identityContext = identityContext;
        _tokenService = tokenService;
        _jwtSettings = jwtSettings.Value;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<AuthResponse?> LoginAsync(AuthRequest request)
    {
        try
        {
            _logger.LogInformation("Login attempt for email: {Email}", request.Email);

            // Find user by email
            var managedUser = await _userManager.FindByEmailAsync(request.Email);
            if (managedUser == null)
            {
                _logger.LogWarning("Login failed: User not found for email: {Email}", request.Email);
                return null;
            }

            // Verify password
            var isPasswordValid = await _userManager.CheckPasswordAsync(managedUser, request.Password);
            if (!isPasswordValid)
            {
                _logger.LogWarning("Login failed: Invalid password for email: {Email}", request.Email);
                return null;
            }

            // Double-check user exists in the Users table (compatibility with old system)
            var userExists = await _identityContext.Users
                .AnyAsync(u => u.Email == request.Email);

            if (!userExists)
            {
                _logger.LogWarning("Login failed: User found in UserManager but not in Users table: {Email}", request.Email);
                return null;
            }

            // Create JWT token
            var accessToken = _tokenService.CreateToken(managedUser);
            var expiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.TokenExpirationMinutes);

            _logger.LogInformation("Login successful for user: {UserId}, email: {Email}",
                managedUser.Id, request.Email);

            return new AuthResponse
            {
                Username = managedUser.UserName ?? string.Empty,
                Email = managedUser.Email ?? string.Empty,
                Token = accessToken,
                ExpiresAt = expiresAt
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for email: {Email}", request.Email);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<(bool Success, IEnumerable<string> Errors)> RegisterAsync(RegistrationRequest request)
    {
        try
        {
            _logger.LogInformation("Registration attempt for email: {Email}, username: {Username}",
                request.Email, request.Username);

            // Check if user already exists
            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
            {
                _logger.LogWarning("Registration failed: User already exists with email: {Email}", request.Email);
                return (false, new[] { "User with this email already exists" });
            }

            // Create new user
            var newUser = new IdentityUser
            {
                UserName = request.Username,
                Email = request.Email,
                EmailConfirmed = false // Can be changed based on requirements
            };

            var result = await _userManager.CreateAsync(newUser, request.Password);

            if (result.Succeeded)
            {
                _logger.LogInformation("Registration successful for user: {UserId}, email: {Email}",
                    newUser.Id, request.Email);
                return (true, Array.Empty<string>());
            }

            var errors = result.Errors.Select(e => e.Description).ToList();
            _logger.LogWarning("Registration failed for email: {Email}. Errors: {Errors}",
                request.Email, string.Join(", ", errors));

            return (false, errors);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration for email: {Email}", request.Email);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<bool> UserExistsAsync(string email)
    {
        try
        {
            var user = await _userManager.FindByEmailAsync(email);
            return user != null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if user exists for email: {Email}", email);
            throw;
        }
    }
}
