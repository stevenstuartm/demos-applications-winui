using System.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using demos_applications_winui.Services;

namespace demos_applications_winui;

public sealed partial class MainWindow : Window
{
    private readonly INavigationService _navigationService;

    public MainWindow(INavigationService navigationService)
    {
        _navigationService = navigationService;

        InitializeComponent();

        ExtendsContentIntoTitleBar = true;
        SetTitleBar(AppTitleBar);

        navigationService.SetFrame(rootFrame);
        navigationService.PropertyChanged += OnNavigationServicePropertyChanged;
    }

    private void OnNavigationServicePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(INavigationService.CanGoBack))
        {
            AppTitleBar.IsBackButtonEnabled = _navigationService.CanGoBack;
        }
    }

    private void AppTitleBar_BackRequested(TitleBar sender, object args)
    {
        _navigationService.GoBack();
    }
}
