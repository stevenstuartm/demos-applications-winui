using System;
using Microsoft.Extensions.DependencyInjection;

namespace demos_applications_winui.Core.Configuration;

public static class ConfigurationServiceCollectionExtensions
{
    public static IServiceCollection AddAppConfig(
        this IServiceCollection services,
        Action<AppConfig> configure)
    {
        var config = new AppConfig();
        configure(config);
        services.AddSingleton(config);
        return services;
    }
}
