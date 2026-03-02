namespace demos_applications_winui.Logging;

/// <summary>
/// Provides the current authentication token for remote log shipping.
/// The token is resolved dynamically from the current user session,
/// returning null when no authenticated session exists.
/// </summary>
public interface ILogAuthTokenProvider
{
    string? GetToken();
}
