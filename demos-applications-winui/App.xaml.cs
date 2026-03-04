using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
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
using demos_applications_winui.Toolkit.Configuration;
using demos_applications_winui.ViewModels;
using demos_applications_winui.Views;
using demos_applications_winui.Toolkit.Navigation;
using demos_applications_winui.Toolkit.Platform;
using demos_applications_winui.Toolkit.Providers;

namespace demos_applications_winui;

/// <summary>
/// Composition root and application entry point. Configures all DI services,
/// wires global exception handlers (UI + background tasks), and triggers
/// initial navigation.
/// </summary>
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
        services.AddNavigationConfig(typeof(DashboardPage), config =>
        {
            config.Guard(NavigationGuardNames.IsAuthenticated).ForAll().Except(typeof(LoginPage));
        });

        // Domain services — registered before logging so remote log infrastructure
        // can resolve IAuthState for dynamic token injection.
        services.AddAuth();
        services.AddNotes();

        // Dashboard (shell-level page, not a domain)
        services.AddTransient<DashboardViewModel>();
        services.AddTransient<DashboardPage>();

        // Logging — JSON-driven Serilog configuration, layered by Stage
        var logDirectory = Environment.GetEnvironmentVariable("APP_LOG_DIRECTORY")
            ?? Path.Combine(ApplicationData.Current.LocalFolder.Path, "logs");

        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
            .AddJsonFile($"appsettings.{hostConfig.Stage}.json", optional: true, reloadOnChange: false)
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Serilog:WriteTo:0:Args:path"] = Path.Combine(logDirectory, "app-.log")
            })
            .AddEnvironmentVariables()
            .Build();

        services.AddLogging(builder =>
        {
            if (hostConfig.Stage == AppStage.Local)
                builder.AddDebug();

            var serilogLogger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                // Remote log shipping — uncomment when a log ingestion endpoint is available.
                // The AuthenticatedHttpClient injects a bearer token from the current user
                // session on every request. See the Logging/ folder for implementation.
                // Requires: using demos_applications_winui.Logging;
                //
                // .WriteTo.Http(
                //     requestUri: "https://logs.example.com/ingest",
                //     httpClient: new AuthenticatedHttpClient(
                //         new LogAuthTokenProvider(
                //             services.BuildServiceProvider()
                //                 .GetRequiredService<Core.Auth.IAuthState>())))
                .CreateLogger();

            builder.AddSerilog(serilogLogger, dispose: true);
        });

        // Toolkit — toast component + platform providers
        services.AddToast();
        services.AddProviders();

        // Navigation + WIP
        services.AddNavigation();
        services.AddWorkInProgress();

        // Window infrastructure
        services.AddSingleton<IWindowHandleProvider>(sp =>
            new WindowHandleProvider(sp.GetRequiredService<MainWindow>()));

        services.AddSingleton<MainWindow>();

        return services.BuildServiceProvider();
    }
}
