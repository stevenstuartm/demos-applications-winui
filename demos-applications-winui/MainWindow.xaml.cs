using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using demos_applications_winui.Services;

namespace demos_applications_winui;

public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        ExtendsContentIntoTitleBar = true;
        SetTitleBar(AppTitleBar);

        var navigationService = App.Current.Services.GetRequiredService<INavigationService>();
        navigationService.SetFrame(rootFrame);
    }

    private void AppTitleBar_BackRequested(TitleBar sender, object args)
    {
        if (rootFrame.CanGoBack)
        {
            rootFrame.GoBack();
        }
    }
}
