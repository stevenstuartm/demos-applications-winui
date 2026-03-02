using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using demos_applications_winui.Auth.Services;
using demos_applications_winui.Core.Auth;
using demos_applications_winui.Toolkit.Navigation;
using demos_applications_winui.Toolkit.Platform;
using demos_applications_winui.Toolkit.Toast;

namespace demos_applications_winui;

public sealed partial class MainWindow : Window
{
    private readonly INavigationService _navigationService;
    private readonly IAuthService _authService;

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

        navigationService.SetFrame(rootFrame);

        rootFrame.Loaded += (_, _) => dialogProvider.SetXamlRoot(rootFrame.XamlRoot);
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
