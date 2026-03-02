using System;
using Microsoft.Extensions.DependencyInjection;

namespace demos_applications_winui.Core.Configuration;

public static class ConfigurationServiceCollectionExtensions
{
    public static IHostConfig AddHostConfig(this IServiceCollection services)
    {
        var config = HostConfigResolver.Resolve();
        services.AddSingleton<IHostConfig>(config);
        return config;
    }

    public static LoggingConfig AddLoggingConfig(this IServiceCollection services)
    {
        var config = LoggingConfigResolver.Resolve();
        services.AddSingleton(config);
        return config;
    }

    public static NavigationConfig AddNavigationConfig(this IServiceCollection services, Type defaultPage, Action<NavigationConfig>? configure = null)
    {
        var config = new NavigationConfig { DefaultPage = defaultPage };
        configure?.Invoke(config);
        services.AddSingleton(config);
        return config;
    }
}
