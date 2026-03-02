using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using demos_applications_winui.Auth.Services;
using demos_applications_winui.Toolkit.Navigation;

namespace demos_applications_winui.Auth.ViewModels;

public partial class LoginViewModel(IAuthService authService) : ObservableObject, INavigable
{
    [ObservableProperty]
    public partial string? Username { get; set; }

    [ObservableProperty]
    public partial string? Password { get; set; }

    [ObservableProperty]
    public partial string? ErrorMessage { get; set; }

    [ObservableProperty]
    public partial bool HasError { get; set; }

    public Task OnNavigatedToAsync(NavigationContext context)
    {
        Username = null;
        Password = null;
        ErrorMessage = null;
        HasError = false;
        return Task.CompletedTask;
    }

    public void OnNavigatedFrom()
    {
        Username = null;
        Password = null;
        ErrorMessage = null;
        HasError = false;
    }

    private bool CanLogin() => !string.IsNullOrWhiteSpace(Username) && !string.IsNullOrWhiteSpace(Password);

    partial void OnUsernameChanged(string? value) => LoginCommand.NotifyCanExecuteChanged();
    partial void OnPasswordChanged(string? value) => LoginCommand.NotifyCanExecuteChanged();

    [RelayCommand(CanExecute = nameof(CanLogin))]
    private async Task LoginAsync()
    {
        ErrorMessage = null;
        HasError = false;

        var result = await authService.LoginAsync(Username!, Password!);

        if (!result.Success)
        {
            ErrorMessage = result.ErrorMessage;
            HasError = true;
        }
    }
}
