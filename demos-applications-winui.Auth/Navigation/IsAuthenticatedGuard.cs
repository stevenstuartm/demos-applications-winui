using demos_applications_winui.Auth.Views;
using demos_applications_winui.Core.Auth;
using demos_applications_winui.Core.Navigation;

namespace demos_applications_winui.Auth.Navigation;

/// <summary>
/// Redirects unauthenticated users to <see cref="LoginPage"/>.
/// Configured globally via guard routing with the login page excluded.
/// </summary>
public class IsAuthenticatedGuard(IAuthState authState) : INavigationGuard
{
    public string Name => NavigationGuardNames.IsAuthenticated;

    public NavigationGuardResult CheckNavigation(NavigationGuardContext context)
    {
        if (!authState.IsAuthenticated)
            return NavigationGuardResult.RedirectTo(typeof(LoginPage));

        return NavigationGuardResult.Allow;
    }
}
