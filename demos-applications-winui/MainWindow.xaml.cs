using System.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using demos_applications_winui.Auth.Services;
using demos_applications_winui.Core.Auth;
using demos_applications_winui.Toolkit.Navigation;
using demos_applications_winui.Toolkit.Platform;
using demos_applications_winui.Toolkit.Toast;
using demos_applications_winui.Views;

namespace demos_applications_winui;

/// <summary>
/// Application shell. Sets up the custom title bar, injects the toast presenter
/// into the visual tree, binds the content frame to <see cref="INavigationService"/>,
/// wires <see cref="IDialogProvider.SetXamlRoot"/> once the frame loads, and
/// provides global navigation via <see cref="NavigationView"/> in Top pane mode.
/// </summary>
public sealed partial class MainWindow : Window
{
    private readonly INavigationService _navigationService;
    private readonly IAuthService _authService;
    private bool _suppressSelectionSync;

    public INavigationState NavigationState { get; }
    public IAuthState AuthState { get; }

    public MainWindow(
        INavigationService navigationService,
        INavigationState navigationState,
        IAuthState authState,
        IAuthService authService,
        IDialogProvider dialogProvider,
        ToastPresenter toastPresenter)
    {
        _navigationService = navigationService;
        _authService = authService;
        NavigationState = navigationState;
        AuthState = authState;

        InitializeComponent();

        ExtendsContentIntoTitleBar = true;
        SetTitleBar(AppTitleBar);

        Grid.SetRow(toastPresenter, 1);
        RootGrid.Children.Add(toastPresenter);

        navigationService.SetFrame(new NavigationFrame(rootFrame));

        rootFrame.Loaded += (_, _) => dialogProvider.SetXamlRoot(rootFrame.XamlRoot);

        navigationState.PropertyChanged += OnNavigationStateChanged;
    }

    private void OnNavigationStateChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(INavigationState.CurrentPageType))
            return;

        _suppressSelectionSync = true;
        AppNav.SelectedItem = NavigationState.CurrentPageType == typeof(DashboardPage)
            ? DashboardNavItem
            : null;
        _suppressSelectionSync = false;
    }

    private async void AppNav_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (_suppressSelectionSync) return;

        if (args.SelectedItem is NavigationViewItem { Tag: "Dashboard" })
        {
            await _navigationService.NavigateToDefaultAsync();
        }
    }

    private async void AppTitleBar_BackRequested(TitleBar sender, object args)
    {
        await _navigationService.GoBackAsync();
    }

    private void LogoutButton_Click(object sender, RoutedEventArgs e)
    {
        _authService.Logout();
    }
}
