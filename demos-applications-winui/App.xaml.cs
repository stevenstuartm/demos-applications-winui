using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml;
using Serilog;
using Windows.Storage;
using demos_applications_winui.Auth;
using demos_applications_winui.Auth.Views;
using demos_applications_winui.Core.Configuration;
using demos_applications_winui.Core.Navigation;
using demos_applications_winui.Notes;
using demos_applications_winui.Notes.Views;
using demos_applications_winui.Toolkit.Configuration;
using demos_applications_winui.Toolkit.Navigation;
using demos_applications_winui.Toolkit.Platform;
using demos_applications_winui.Toolkit.Providers;

namespace demos_applications_winui;

public partial class App : Application
{
    private Window? _window;

    public IServiceProvider Services { get; }

    public static new App Current => (App)Application.Current;

    public App()
    {
        Services = ConfigureServices();
        InitializeComponent();

        UnhandledException += OnUnhandledException;
        TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
    }

    protected override async void OnLaunched(LaunchActivatedEventArgs args)
    {
        _window = Services.GetRequiredService<MainWindow>();
        _window.Activate();

        // Run async startup config providers (API-sourced configs, etc.)
        var providers = Services.GetServices<IStartupConfigProvider>()
            .OrderBy(p => p.Order);
        foreach (var provider in providers)
        {
            await provider.LoadAsync();
        }

        var navigationService = Services.GetRequiredService<INavigationService>();
        await navigationService.NavigateToDefaultAsync();
    }

    private void OnUnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
    {
        var logger = Services.GetRequiredService<ILogger<App>>();
        LogUnhandledUiException(logger, e.Exception);

        e.Handled = true;

        var toastProvider = Services.GetRequiredService<IToastProvider>();
        toastProvider.ShowError("Something went wrong", "An unexpected error occurred. Please try again.");
    }

    private void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        var logger = Services.GetRequiredService<ILogger<App>>();
        LogUnobservedTaskException(logger, e.Exception);

        e.SetObserved();

        _window?.DispatcherQueue.TryEnqueue(() =>
        {
            var toastProvider = Services.GetRequiredService<IToastProvider>();
            toastProvider.ShowError("Something went wrong", "A background error occurred.");
        });
    }

    [LoggerMessage(EventId = 7000, Level = LogLevel.Critical, Message = "Unhandled UI exception")]
    static partial void LogUnhandledUiException(Microsoft.Extensions.Logging.ILogger logger, Exception ex);

    [LoggerMessage(EventId = 7001, Level = LogLevel.Critical, Message = "Unobserved task exception")]
    static partial void LogUnobservedTaskException(Microsoft.Extensions.Logging.ILogger logger, Exception ex);

    private static IServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();

        // Configuration — each AddXConfig() calls its resolver and registers the result
        var hostConfig = services.AddHostConfig();
        var loggingConfig = services.AddLoggingConfig();
        services.AddNavigationConfig(typeof(AllNotesPage), config =>
        {
            config.Guard(NavigationGuardNames.IsAuthenticated).ForAll().Except(typeof(LoginPage));
        });

        // Logging — strategy driven by Stage
        var logDir = loggingConfig.LogDirectory
            ?? Path.Combine(ApplicationData.Current.LocalFolder.Path, "logs");

        services.AddLogging(builder =>
        {
            builder.SetMinimumLevel(hostConfig.Stage == AppStage.Local
                ? LogLevel.Debug
                : LogLevel.Information);

            if (hostConfig.Stage == AppStage.Local)
                builder.AddDebug();

            var serilogLogger = new LoggerConfiguration()
                .WriteTo.File(
                    Path.Combine(logDir, loggingConfig.FileNameTemplate ?? "app-.log"),
                    rollingInterval: RollingInterval.Day,
                    fileSizeLimitBytes: loggingConfig.FileSizeLimitBytes ?? 10 * 1024 * 1024,
                    retainedFileCountLimit: loggingConfig.RetainedFileCountLimit ?? 14)
                .CreateLogger();

            builder.AddSerilog(serilogLogger, dispose: true);

            // Stage/Prod: add remote sink here when ready
            // if (hostConfig.Stage != AppStage.Local)
            // {
            //     builder.AddSerilog(remoteSerilogLogger, dispose: true);
            // }
        });

        // Toolkit — toast component + platform providers
        services.AddToast();
        services.AddProviders();

        // Domain services
        services.AddNavigation();
        services.AddAuth();
        services.AddNotes();

        // Window infrastructure
        services.AddSingleton<IWindowHandleProvider>(sp =>
            new WindowHandleProvider(sp.GetRequiredService<MainWindow>()));

        services.AddSingleton<MainWindow>();

        return services.BuildServiceProvider();
    }
}
