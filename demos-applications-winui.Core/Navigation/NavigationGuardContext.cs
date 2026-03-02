using System;

namespace demos_applications_winui.Core.Navigation;

/// <summary>
/// Context passed to <see cref="INavigationGuard.CheckNavigation"/> providing
/// the target page, the current (source) page, and any navigation parameter.
/// </summary>
public record NavigationGuardContext(
    Type TargetPageType,
    Type? SourcePageType,
    object? Parameter);
