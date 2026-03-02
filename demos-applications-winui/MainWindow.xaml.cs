using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using demos_applications_winui.Auth.Services;
using demos_applications_winui.Core.Auth;
using demos_applications_winui.Core.Navigation;

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
        IAuthService authService)
    {
        _navigationService = navigationService;
        _authService = authService;
        NavigationState = navigationState;
        AuthState = authState;

        InitializeComponent();

        ExtendsContentIntoTitleBar = true;
        SetTitleBar(AppTitleBar);

        navigationService.SetFrame(rootFrame);
    }

    private void AppTitleBar_BackRequested(TitleBar sender, object args)
    {
        _navigationService.GoBack();
    }

    private void LogoutButton_Click(object sender, RoutedEventArgs e)
    {
        _authService.Logout();
    }
}
