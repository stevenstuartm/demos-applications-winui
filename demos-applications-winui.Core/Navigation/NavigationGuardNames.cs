namespace demos_applications_winui.Core.Navigation;

/// <summary>
/// Well-known guard name constants used to match <see cref="INavigationGuard.Name"/>
/// with the guard routing configured in <see cref="Configuration.NavigationConfig"/>.
/// Domains define their guards with these names; the composition root maps them to pages.
/// </summary>
public static class NavigationGuardNames
{
    public const string IsAuthenticated = "IsAuthenticated";
}
