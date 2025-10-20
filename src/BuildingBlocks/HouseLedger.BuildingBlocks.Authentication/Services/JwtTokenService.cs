using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using HouseLedger.BuildingBlocks.Authentication.Configuration;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace HouseLedger.BuildingBlocks.Authentication.Services;

/// <summary>
/// JWT token service implementation.
/// Creates and validates JWT tokens for user authentication.
/// </summary>
public class JwtTokenService : ITokenService
{
    private readonly JwtSettings _jwtSettings;
    private readonly ILogger<JwtTokenService> _logger;

    public JwtTokenService(
        IOptions<JwtSettings> jwtSettings,
        ILogger<JwtTokenService> logger)
    {
        _jwtSettings = jwtSettings.Value;
        _logger = logger;
    }

    /// <inheritdoc />
    public string CreateToken(IdentityUser user)
    {
        return CreateToken(user, new Dictionary<string, string>());
    }

    /// <inheritdoc />
    public string CreateToken(IdentityUser user, Dictionary<string, string> additionalClaims)
    {
        ArgumentNullException.ThrowIfNull(user);

        try
        {
            _logger.LogDebug("Creating JWT token for user: {UserId}", user.Id);

            var expiration = DateTime.UtcNow.AddMinutes(_jwtSettings.TokenExpirationMinutes);
            var claims = CreateClaims(user, additionalClaims);
            var signingCredentials = CreateSigningCredentials();
            var token = CreateJwtSecurityToken(claims, signingCredentials, expiration);

            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenString = tokenHandler.WriteToken(token);

            _logger.LogInformation("JWT token created successfully for user: {UserId}, expires at: {Expiration}",
                user.Id, expiration);

            return tokenString;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating JWT token for user: {UserId}", user.Id);
            throw;
        }
    }

    /// <summary>
    /// Creates the JWT security token.
    /// </summary>
    private JwtSecurityToken CreateJwtSecurityToken(
        List<Claim> claims,
        SigningCredentials credentials,
        DateTime expiration)
    {
        return new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: expiration,
            signingCredentials: credentials);
    }

    /// <summary>
    /// Creates the claims for the JWT token.
    /// </summary>
    private List<Claim> CreateClaims(IdentityUser user, Dictionary<string, string> additionalClaims)
    {
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat, DateTime.UtcNow.ToString(CultureInfo.InvariantCulture)),
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Name, user.UserName ?? string.Empty),
            new Claim(ClaimTypes.Email, user.Email ?? string.Empty)
        };

        // Add any additional claims
        foreach (var claim in additionalClaims)
        {
            claims.Add(new Claim(claim.Key, claim.Value));
        }

        return claims;
    }

    /// <summary>
    /// Creates the signing credentials using the secret key.
    /// </summary>
    private SigningCredentials CreateSigningCredentials()
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
        return new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
    }
}
