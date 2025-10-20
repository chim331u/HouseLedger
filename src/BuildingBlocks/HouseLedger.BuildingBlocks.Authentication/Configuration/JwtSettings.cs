namespace HouseLedger.BuildingBlocks.Authentication.Configuration;

/// <summary>
/// JWT authentication settings.
/// Bind this from appsettings.json "JwtSettings" section.
/// </summary>
public class JwtSettings
{
    public const string SectionName = "JwtSettings";

    /// <summary>
    /// The issuer of the JWT token (typically the API name).
    /// </summary>
    public string Issuer { get; set; } = string.Empty;

    /// <summary>
    /// The audience of the JWT token (typically the client application).
    /// </summary>
    public string Audience { get; set; } = string.Empty;

    /// <summary>
    /// The secret key used to sign JWT tokens.
    /// MUST be at least 256 bits (32 characters) for HS256.
    /// Store this in User Secrets (dev) or Key Vault (prod).
    /// </summary>
    public string SecretKey { get; set; } = string.Empty;

    /// <summary>
    /// Token expiration time in minutes.
    /// Default: 30 minutes.
    /// </summary>
    public int TokenExpirationMinutes { get; set; } = 30;

    /// <summary>
    /// Refresh token expiration time in days (if using refresh tokens).
    /// Default: 7 days.
    /// </summary>
    public int RefreshTokenExpirationDays { get; set; } = 7;

    /// <summary>
    /// Validates that the JWT settings are properly configured.
    /// </summary>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Issuer))
            throw new InvalidOperationException("JwtSettings:Issuer is required");

        if (string.IsNullOrWhiteSpace(Audience))
            throw new InvalidOperationException("JwtSettings:Audience is required");

        if (string.IsNullOrWhiteSpace(SecretKey))
            throw new InvalidOperationException("JwtSettings:SecretKey is required");

        if (SecretKey.Length < 32)
            throw new InvalidOperationException("JwtSettings:SecretKey must be at least 32 characters (256 bits) for HS256");

        if (TokenExpirationMinutes <= 0)
            throw new InvalidOperationException("JwtSettings:TokenExpirationMinutes must be greater than 0");
    }
}
