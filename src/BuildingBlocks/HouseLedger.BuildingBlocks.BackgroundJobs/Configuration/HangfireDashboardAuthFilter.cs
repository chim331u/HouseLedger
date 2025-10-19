using Hangfire.Dashboard;
using Microsoft.Extensions.Options;

namespace HouseLedger.BuildingBlocks.BackgroundJobs.Configuration;

/// <summary>
/// Authorization filter for Hangfire dashboard with basic authentication.
/// </summary>
public class HangfireDashboardAuthFilter : IDashboardAuthorizationFilter
{
    private readonly HangfireOptions _options;

    public HangfireDashboardAuthFilter(IOptions<HangfireOptions> options)
    {
        _options = options.Value;
    }

    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();

        // If no credentials configured, allow access (development mode)
        if (string.IsNullOrEmpty(_options.DashboardUsername) ||
            string.IsNullOrEmpty(_options.DashboardPassword))
        {
            return true;
        }

        // Check for Authorization header
        var authHeader = httpContext.Request.Headers["Authorization"].ToString();

        if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Basic "))
        {
            // Send 401 with WWW-Authenticate header
            httpContext.Response.StatusCode = 401;
            httpContext.Response.Headers["WWW-Authenticate"] = "Basic realm=\"Hangfire Dashboard\"";
            return false;
        }

        try
        {
            // Decode Basic auth credentials
            var encodedCredentials = authHeader.Substring("Basic ".Length).Trim();
            var credentialBytes = Convert.FromBase64String(encodedCredentials);
            var credentials = System.Text.Encoding.UTF8.GetString(credentialBytes).Split(':', 2);

            if (credentials.Length != 2)
            {
                return false;
            }

            var username = credentials[0];
            var password = credentials[1];

            // Simple username/password check
            return username == _options.DashboardUsername &&
                   password == _options.DashboardPassword;
        }
        catch
        {
            return false;
        }
    }
}
