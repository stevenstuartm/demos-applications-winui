using System;
using Microsoft.Extensions.DependencyInjection;

namespace demos_applications_winui.Core.Configuration;

/// <summary>
/// DI registration extensions for Core configuration singletons.
/// Each method resolves its config (from env vars or defaults), registers it,
/// and returns the instance so the composition root can use it immediately.
/// </summary>
public static class ConfigurationServiceCollectionExtensions
{
    /// <summary>
    /// Resolves <see cref="HostConfig"/> from environment variables and registers
    /// it as <see cref="IHostConfig"/>. Returns the resolved config for
    /// use during remaining startup (e.g., selecting logging strategy).
    /// </summary>
    public static IHostConfig AddHostConfig(this IServiceCollection services)
    {
        var config = HostConfigResolver.Resolve();
        services.AddSingleton<IHostConfig>(config);
        return config;
    }

    /// <summary>
    /// Creates a <see cref="NavigationConfig"/> with the given default page,
    /// applies the optional guard routing configuration, and registers it.
    /// </summary>
    public static NavigationConfig AddNavigationConfig(this IServiceCollection services, Type defaultPage, Action<NavigationConfig>? configure = null)
    {
        var config = new NavigationConfig { DefaultPage = defaultPage };
        configure?.Invoke(config);
        services.AddSingleton(config);
        return config;
    }
}
