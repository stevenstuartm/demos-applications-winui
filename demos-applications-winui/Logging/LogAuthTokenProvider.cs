using System;
using System.Text;
using demos_applications_winui.Core.Auth;

namespace demos_applications_winui.Logging;

/// <summary>
/// Resolves a bearer token from the current authentication state.
/// In the mock auth system this synthesizes a placeholder token from the username.
/// Replace with real token acquisition (e.g., MSAL, OAuth refresh) for production use.
/// </summary>
public class LogAuthTokenProvider(IAuthState authState) : ILogAuthTokenProvider
{
    public string? GetToken()
    {
        if (!authState.IsAuthenticated || authState.Username is null)
            return null;

        // Mock: synthesize a placeholder token from the username.
        // Production: return the cached access token from your identity provider,
        // e.g., _msalClient.AcquireTokenSilent(...) or a cached JWT from IAuthService.
        return Convert.ToBase64String(
            Encoding.UTF8.GetBytes($"log-session:{authState.Username}:{DateTime.UtcNow:O}"));
    }
}
