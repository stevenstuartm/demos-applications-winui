using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using demos_applications_winui.Auth.Models;
using demos_applications_winui.Auth.Views;
using demos_applications_winui.Core.Navigation;

namespace demos_applications_winui.Auth.Services;

public partial class AuthService(
    AuthState AuthState,
    INavigationService navigationService,
    ILogger<AuthService> logger) : IAuthService
{
    private const string ValidUsername = "admin";
    private const string ValidPassword = "password";

    public async Task<AuthResult> LoginAsync(string username, string password)
    {
        LogLoginAttempt(username);

        if (string.Equals(username, ValidUsername, StringComparison.OrdinalIgnoreCase)
            && password == ValidPassword)
        {
            AuthState.SetAuthenticated(username);
            await navigationService.NavigateToDefaultAsync();
            LogLoginSuccess(username);
            return new AuthResult(true);
        }

        LogLoginFailed(username);
        return new AuthResult(false, "Invalid username or password.");
    }

    public async void Logout()
    {
        LogLogout(AuthState.Username);
        AuthState.Clear();
        await navigationService.NavigateAndReplaceAsync<LoginPage>();
    }

    [LoggerMessage(EventId = 2000, Level = LogLevel.Information, Message = "Login attempt for user {Username}")]
    partial void LogLoginAttempt(string username);

    [LoggerMessage(EventId = 2001, Level = LogLevel.Information, Message = "User {Username} logged in")]
    partial void LogLoginSuccess(string username);

    [LoggerMessage(EventId = 2002, Level = LogLevel.Warning, Message = "Login failed for user {Username}")]
    partial void LogLoginFailed(string username);

    [LoggerMessage(EventId = 2003, Level = LogLevel.Information, Message = "User {Username} logged out")]
    partial void LogLogout(string? username);
}
