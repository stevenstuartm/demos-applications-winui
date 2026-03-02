using demos_applications_winui.Auth.Models;
using demos_applications_winui.Auth.Navigation;
using demos_applications_winui.Auth.Services;
using demos_applications_winui.Auth.ViewModels;
using demos_applications_winui.Auth.Views;
using demos_applications_winui.Core.Auth;
using demos_applications_winui.Core.Navigation;
using Microsoft.Extensions.DependencyInjection;

namespace demos_applications_winui.Auth;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAuth(this IServiceCollection services)
    {
        services.AddSingleton<AuthState>();
        services.AddSingleton<IAuthState>(sp => sp.GetRequiredService<AuthState>());

        services.AddSingleton<IAuthService, AuthService>();
        services.AddSingleton<INavigationGuard, IsAuthenticatedGuard>();

        services.AddSingleton<LoginViewModel>();
        services.AddSingleton<LoginPage>();

        return services;
    }
}
