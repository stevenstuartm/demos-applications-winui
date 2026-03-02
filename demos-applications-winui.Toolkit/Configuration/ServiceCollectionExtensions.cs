using Microsoft.Extensions.DependencyInjection;
using demos_applications_winui.Core.Platform;
using demos_applications_winui.Toolkit.Providers;
using demos_applications_winui.Toolkit.Toast;

namespace demos_applications_winui.Toolkit.Configuration;

public static class ToolkitServiceCollectionExtensions
{
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
}
