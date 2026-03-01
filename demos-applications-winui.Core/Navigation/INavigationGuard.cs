using System;

namespace demos_applications_winui.Core.Navigation;

public interface INavigationGuard
{
    NavigationGuardResult CheckNavigation(Type pageType);
}

public record NavigationGuardResult
{
    public bool Allowed { get; init; }
    public Type? RedirectPageType { get; init; }

    public static readonly NavigationGuardResult Allow = new() { Allowed = true };

    public static NavigationGuardResult RedirectTo(Type pageType)
        => new() { Allowed = false, RedirectPageType = pageType };
}
