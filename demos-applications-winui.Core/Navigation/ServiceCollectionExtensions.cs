using Microsoft.Extensions.DependencyInjection;

namespace demos_applications_winui.Core.Navigation;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddNavigation(this IServiceCollection services)
    {
        services.AddSingleton<NavigationState>();
        services.AddSingleton<INavigationState>(sp => sp.GetRequiredService<NavigationState>());
        services.AddSingleton<INavigationService, NavigationService>();

        return services;
    }
}
