using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using demos_applications_winui.Auth;
using demos_applications_winui.Auth.Views;
using demos_applications_winui.Core.Configuration;
using demos_applications_winui.Core.Navigation;
using demos_applications_winui.Notes;
using demos_applications_winui.Notes.Views;

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
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        _window = Services.GetRequiredService<MainWindow>();
        _window.Activate();

        var navigationService = Services.GetRequiredService<INavigationService>();
        navigationService.NavigateToDefault();
    }

    private static IServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();

        services.AddAppConfig(config =>
        {
            config.Navigation.DefaultPage = typeof(AllNotesPage);
            config.Navigation
                .Guard(NavigationGuardNames.IsAuthenticated).ForAll().Except(typeof(LoginPage));
        });

        services.AddNavigation();
        services.AddAuth();
        services.AddNotes();

        services.AddSingleton<MainWindow>();

        return services.BuildServiceProvider();
    }
}
