using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using demos_applications_winui.Services;
using demos_applications_winui.ViewModels;
using demos_applications_winui.Views;

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

        services.AddSingleton<INotesService, NotesService>();
        services.AddSingleton<INavigationService, NavigationService>();

        services.AddSingleton<AllNotesViewModel>();
        services.AddSingleton<NoteViewModel>();

        services.AddSingleton<AllNotesPage>();
        services.AddSingleton<NotePage>();
        services.AddSingleton<MainWindow>();

        return services.BuildServiceProvider();
    }
}
