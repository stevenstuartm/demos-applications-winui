using demos_applications_winui.Auth.Models;
using demos_applications_winui.Auth.Navigation;
using demos_applications_winui.Auth.Services;
using demos_applications_winui.Auth.ViewModels;
using demos_applications_winui.Auth.Views;
using demos_applications_winui.Core.Auth;
using demos_applications_winui.Core.Navigation;
using Microsoft.Extensions.DependencyInjection;

namespace demos_applications_winui.Auth;

/// <summary>
/// Registers all Auth domain services: <see cref="AuthState"/>/<see cref="IAuthState"/>,
/// <see cref="IAuthService"/>, the <see cref="IsAuthenticatedGuard"/>, and the login page/VM.
/// </summary>
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAuth(this IServiceCollection services)
    {
        services.AddSingleton<AuthState>();
        services.AddSingleton<IAuthState>(sp => sp.GetRequiredService<AuthState>());

        services.AddSingleton<IAuthService, AuthService>();
        services.AddSingleton<INavigationGuard, IsAuthenticatedGuard>();

        services.AddTransient<LoginViewModel>();
        services.AddTransient<LoginPage>();

        return services;
    }
}
