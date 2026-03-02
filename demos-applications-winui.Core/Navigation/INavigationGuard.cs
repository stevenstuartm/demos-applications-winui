namespace demos_applications_winui.Core.Navigation;

/// <summary>
/// Pluggable navigation interceptor that can block or redirect navigation.
/// Guards are registered in DI and matched to pages by <see cref="Name"/>
/// against the guard routing configured in <see cref="Configuration.NavigationConfig"/>.
/// </summary>
public interface INavigationGuard
{
    /// <summary>
    /// Identifies this guard for matching against <see cref="NavigationGuardNames"/> constants
    /// configured via the fluent guard routing API.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Evaluates whether navigation should proceed, be blocked, or be redirected.
    /// Only called when this guard's <see cref="Name"/> matches a guard configured
    /// for the target page.
    /// </summary>
    NavigationGuardResult CheckNavigation(NavigationGuardContext context);
}
