namespace HouseLedger.BuildingBlocks.Authentication.Contract;

/// <summary>
/// Response model for successful authentication.
/// </summary>
public class AuthResponse
{
    /// <summary>
    /// The user's username.
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// The user's email address.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// The JWT bearer token.
    /// </summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// Token expiration time in UTC.
    /// </summary>
    public DateTime ExpiresAt { get; set; }
}
