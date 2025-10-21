using System.ComponentModel.DataAnnotations;

namespace HouseLedger.BuildingBlocks.Authentication.Contract;

/// <summary>
/// Request model for user authentication.
/// </summary>
public class AuthRequest
{
    /// <summary>
    /// The user's email address.
    /// </summary>
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email address")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// The user's password.
    /// </summary>
    [Required(ErrorMessage = "Password is required")]
    public string Password { get; set; } = string.Empty;
}
