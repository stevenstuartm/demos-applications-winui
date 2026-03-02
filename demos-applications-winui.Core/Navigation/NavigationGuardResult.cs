using System;

namespace demos_applications_winui.Core.Navigation;

/// <summary>
/// Result returned by <see cref="INavigationGuard.CheckNavigation"/>.
/// Either allows navigation, blocks it, or redirects to a different page.
/// </summary>
public record NavigationGuardResult
{
    public bool Allowed { get; init; }
    public Type? RedirectPageType { get; init; }

    public static readonly NavigationGuardResult Allow = new() { Allowed = true };

    public static NavigationGuardResult RedirectTo(Type pageType)
        => new() { Allowed = false, RedirectPageType = pageType };
}
