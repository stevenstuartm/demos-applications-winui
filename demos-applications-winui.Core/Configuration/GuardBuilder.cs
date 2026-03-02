using System;
using System.Linq;

namespace demos_applications_winui.Core.Configuration;

/// <summary>
/// Fluent builder for configuring which pages a named guard applies to.
/// Created by <see cref="NavigationConfig.Guard"/>. Must be terminated
/// with either <see cref="ForAll"/> (global) or <see cref="ForPages"/> (targeted).
/// </summary>
public class GuardBuilder
{
    private readonly NavigationConfig _config;
    private readonly string _guardName;

    internal GuardBuilder(NavigationConfig config, string guardName)
    {
        _config = config;
        _guardName = guardName;
    }

    /// <summary>
    /// Applies the guard to all pages. Optionally chain <see cref="GlobalGuardBuilder.Except"/>
    /// to exclude specific pages (e.g. a login page that must be accessible without auth).
    /// </summary>
    public GlobalGuardBuilder ForAll()
    {
        var registration = new GuardRegistration
        {
            GuardName = _guardName,
            IsGlobal = true
        };
        _config.AddGuard(registration);
        return new GlobalGuardBuilder(_config, registration);
    }

    /// <summary>
    /// Applies the guard only to the specified pages. Use for targeted guards
    /// like role checks or dirty-form protection that apply to a subset of pages.
    /// </summary>
    public NavigationConfig ForPages(params Type[] pages)
    {
        _config.AddGuard(new GuardRegistration
        {
            GuardName = _guardName,
            IsGlobal = false,
            IncludedPages = pages.ToHashSet()
        });
        return _config;
    }
}

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
