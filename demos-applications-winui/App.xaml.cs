using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using demos_applications_winui.Core.Navigation;
using demos_applications_winui.Notes;
using demos_applications_winui.Notes.Views;
using demos_applications_winui.Services;

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
        navigationService.NavigateTo<AllNotesPage>();
    }

    private static IServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();

        services.AddNotes();
        services.AddSingleton<INavigationService, NavigationService>();

        services.AddSingleton<MainWindow>();

        return services.BuildServiceProvider();
    }
}
