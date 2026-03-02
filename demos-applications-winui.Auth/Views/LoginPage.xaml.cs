using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using demos_applications_winui.Auth.ViewModels;
using demos_applications_winui.Core.Navigation;

namespace demos_applications_winui.Auth.Views;

public sealed partial class LoginPage : Page, INavigable
{
    public LoginViewModel ViewModel { get; }

    public LoginPage(LoginViewModel viewModel)
    {
        ViewModel = viewModel;
        InitializeComponent();
    }

    public Task OnNavigatedToAsync(NavigationContext context) => ViewModel.OnNavigatedToAsync(context);

    public void OnNavigatedFrom()
    {
        PasswordInput.Password = string.Empty;
        ViewModel.OnNavigatedFrom();
    }

    private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (sender is PasswordBox pb)
            ViewModel.Password = pb.Password;
    }
}
