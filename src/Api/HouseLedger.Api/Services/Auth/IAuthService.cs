using HouseLedger.Api.Models.Auth;

namespace HouseLedger.Api.Services.Auth;

/// <summary>
/// Service for handling user authentication and registration.
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Authenticates a user with email and password.
    /// </summary>
    /// <param name="request">The authentication request.</param>
    /// <returns>Authentication response with JWT token, or null if authentication failed.</returns>
    Task<AuthResponse?> LoginAsync(AuthRequest request);

    /// <summary>
    /// Registers a new user.
    /// </summary>
    /// <param name="request">The registration request.</param>
    /// <returns>True if registration succeeded, false otherwise.</returns>
    Task<(bool Success, IEnumerable<string> Errors)> RegisterAsync(RegistrationRequest request);

    /// <summary>
    /// Checks if a user with the given email exists.
    /// </summary>
    /// <param name="email">The email to check.</param>
    /// <returns>True if user exists, false otherwise.</returns>
    Task<bool> UserExistsAsync(string email);
}
