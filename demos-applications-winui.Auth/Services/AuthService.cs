using System;
using System.Threading.Tasks;
using demos_applications_winui.Auth.Models;
using demos_applications_winui.Auth.Views;
using demos_applications_winui.Core.Navigation;

namespace demos_applications_winui.Auth.Services;

public class AuthService(AuthState AuthState, INavigationService navigationService) : IAuthService
{
    private const string ValidUsername = "admin";
    private const string ValidPassword = "password";

    public Task<AuthResult> LoginAsync(string username, string password)
    {
        if (string.Equals(username, ValidUsername, StringComparison.OrdinalIgnoreCase)
            && password == ValidPassword)
        {
            AuthState.SetAuthenticated(username);
            navigationService.NavigateToDefault();
            return Task.FromResult(new AuthResult(true));
        }

        return Task.FromResult(new AuthResult(false, "Invalid username or password."));
    }

    public void Logout()
    {
        AuthState.Clear();
        navigationService.NavigateAndReplace<LoginPage>();
    }
}
