using System;
using System.Collections.Generic;

namespace demos_applications_winui.Core.Configuration;

/// <summary>
/// Configures navigation behavior including the default page and guard routing.
/// Guard routing is configured via a fluent API and resolved at navigation time
/// without reflection — all mappings are built at startup from explicit configuration.
/// </summary>
/// <example>
/// config.Navigation.DefaultPage = typeof(AllNotesPage);
/// config.Navigation
///     .Guard(NavigationGuardNames.IsAuthenticated).ForAll().Except(typeof(LoginPage))
///     .Guard(NavigationGuardNames.AdminRole).ForPages(typeof(AdminPage));
/// </example>
public class NavigationConfig
{
    public required Type DefaultPage { get; set; }

    private readonly List<GuardRegistration> _guards = [];
    private readonly Dictionary<Type, HashSet<string>> _resolvedCache = [];

    /// <summary>
    /// Begins configuring a navigation guard by name. Chain with
    /// <see cref="GuardBuilder.ForAll"/> or <see cref="GuardBuilder.ForPages"/>
    /// to specify which pages the guard applies to.
    /// </summary>
    public GuardBuilder Guard(string guardName) => new(this, guardName);

    internal void AddGuard(GuardRegistration registration) => _guards.Add(registration);

    /// <summary>
    /// Resolves which guards apply to a given page type by matching against
    /// all configured guard registrations. Results are cached per page type
    /// since the configuration is immutable after startup.
    /// </summary>
    public HashSet<string> GetGuardsForPage(Type pageType)
    {
        if (_resolvedCache.TryGetValue(pageType, out var cached))
            return cached;

        var guards = new HashSet<string>(StringComparer.Ordinal);

        foreach (var reg in _guards)
        {
            if (reg.IsGlobal)
            {
                if (reg.ExcludedPages?.Contains(pageType) != true)
                    guards.Add(reg.GuardName);
            }
            else
            {
                if (reg.IncludedPages?.Contains(pageType) == true)
                    guards.Add(reg.GuardName);
            }
        }

        _resolvedCache[pageType] = guards;
        return guards;
    }
}
