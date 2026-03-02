using System;
using System.Linq;

namespace demos_applications_winui.Core.Configuration;

/// <summary>
/// Continuation builder for a global guard, allowing specific pages to be
/// excluded. Created by <see cref="GuardBuilder.ForAll"/>.
/// Returns <see cref="NavigationConfig"/> to allow chaining additional guards.
/// </summary>
public class GlobalGuardBuilder
{
    private readonly NavigationConfig _config;
    private readonly GuardRegistration _registration;

    internal GlobalGuardBuilder(NavigationConfig config, GuardRegistration registration)
    {
        _config = config;
        _registration = registration;
    }

    /// <summary>
    /// Excludes the specified pages from this global guard. These pages will
    /// not be evaluated by this guard during navigation.
    /// </summary>
    public NavigationConfig Except(params Type[] pages)
    {
        _registration.ExcludedPages = pages.ToHashSet();
        return _config;
    }
}
