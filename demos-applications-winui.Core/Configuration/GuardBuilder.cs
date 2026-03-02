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
            IsGlobal = true,
            ExcludedPages = new System.Collections.Generic.HashSet<Type>()
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
