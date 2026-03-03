using Microsoft.Extensions.DependencyInjection;
using demos_applications_winui.Toolkit.Navigation;
using demos_applications_winui.Toolkit.Platform;
using demos_applications_winui.Toolkit.Providers;
using demos_applications_winui.Toolkit.Toast;
using demos_applications_winui.Toolkit.WorkInProgress;

namespace demos_applications_winui.Toolkit.Configuration;

/// <summary>
/// DI registration extensions for Toolkit infrastructure. Each method registers
/// a cohesive group of services as singletons with read-only state forwarding
/// (e.g., <c>NavigationState</c> → <c>INavigationState</c>).
/// </summary>
public static class ToolkitServiceCollectionExtensions
{
    /// <summary>
    /// Registers <see cref="NavigationService"/>, <see cref="NavigationState"/>,
    /// and the read-only <see cref="INavigationState"/> projection.
    /// </summary>
    public static IServiceCollection AddNavigation(this IServiceCollection services)
    {
        services.AddSingleton<NavigationState>();
        services.AddSingleton<INavigationState>(sp => sp.GetRequiredService<NavigationState>());
        services.AddSingleton<INavigationService, NavigationService>();
        return services;
    }

    public static IServiceCollection AddToast(this IServiceCollection services)
    {
        services.AddSingleton<ToastState>();
        services.AddSingleton<IToastState>(sp => sp.GetRequiredService<ToastState>());
        services.AddSingleton<IToastProvider, ToastProvider>();
        services.AddSingleton<ToastPresenter>();
        return services;
    }

    public static IServiceCollection AddProviders(this IServiceCollection services)
    {
        services.AddSingleton<IDialogProvider, DialogProvider>();
        services.AddSingleton<IFilePickerProvider, FilePickerProvider>();
        services.AddSingleton<ICameraProvider, CameraProvider>();
        return services;
    }

    /// <summary>
    /// Registers the <see cref="IWorkInProgressRepository"/> singleton. Sessions
    /// are created on-demand by key — no per-type registration needed.
    /// </summary>
    public static IServiceCollection AddWorkInProgress(this IServiceCollection services)
    {
        services.AddSingleton<IWorkInProgressRepository, WorkInProgressRepository>();
        return services;
    }
}
