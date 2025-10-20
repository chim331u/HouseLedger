using Microsoft.AspNetCore.Identity;

namespace HouseLedger.BuildingBlocks.Authentication.Services;

/// <summary>
/// Service for creating and managing JWT tokens.
/// </summary>
public interface ITokenService
{
    /// <summary>
    /// Creates a JWT token for the specified user.
    /// </summary>
    /// <param name="user">The user to create a token for.</param>
    /// <returns>The JWT token string.</returns>
    string CreateToken(IdentityUser user);

    /// <summary>
    /// Creates a JWT token for the specified user with additional claims.
    /// </summary>
    /// <param name="user">The user to create a token for.</param>
    /// <param name="additionalClaims">Additional claims to include in the token.</param>
    /// <returns>The JWT token string.</returns>
    string CreateToken(IdentityUser user, Dictionary<string, string> additionalClaims);
}
