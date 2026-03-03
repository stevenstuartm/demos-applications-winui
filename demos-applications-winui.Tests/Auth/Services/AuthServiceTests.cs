using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using demos_applications_winui.Auth.Models;
using demos_applications_winui.Auth.Services;
using demos_applications_winui.Toolkit.Navigation;
using Xunit;

namespace demos_applications_winui.Tests.Auth.Services;

public class AuthServiceTests
{
    private static (AuthService service, AuthState authState, INavigationService nav) CreateService()
    {
        var authState = new AuthState();
        var nav = Substitute.For<INavigationService>();
        var service = new AuthService(authState, nav, NullLogger<AuthService>.Instance);
        return (service, authState, nav);
    }

    [Fact]
    public async Task LoginAsync_ValidCredentials_ReturnsSuccess()
    {
        var (service, _, _) = CreateService();

        var result = await service.LoginAsync("admin", "password");

        result.Success.Should().BeTrue();
        result.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public async Task LoginAsync_ValidCredentials_SetsAuthState()
    {
        var (service, authState, _) = CreateService();

        await service.LoginAsync("admin", "password");

        authState.IsAuthenticated.Should().BeTrue();
        authState.Username.Should().Be("admin");
    }

    [Fact]
    public async Task LoginAsync_ValidCredentials_NavigatesToDefault()
    {
        var (service, _, nav) = CreateService();

        await service.LoginAsync("admin", "password");

        await nav.Received(1).NavigateToDefaultAsync();
    }

    [Fact]
    public async Task LoginAsync_ValidCredentials_CaseInsensitiveUsername()
    {
        var (service, authState, _) = CreateService();

        var result = await service.LoginAsync("Admin", "password");

        result.Success.Should().BeTrue();
        authState.IsAuthenticated.Should().BeTrue();
    }

    [Fact]
    public async Task LoginAsync_InvalidPassword_ReturnsFailure()
    {
        var (service, _, _) = CreateService();

        var result = await service.LoginAsync("admin", "wrong");

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task LoginAsync_InvalidUsername_ReturnsFailure()
    {
        var (service, _, _) = CreateService();

        var result = await service.LoginAsync("unknown", "password");

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task LoginAsync_InvalidCredentials_DoesNotSetAuthState()
    {
        var (service, authState, _) = CreateService();

        await service.LoginAsync("wrong", "wrong");

        authState.IsAuthenticated.Should().BeFalse();
        authState.Username.Should().BeNull();
    }

    [Fact]
    public async Task LoginAsync_InvalidCredentials_DoesNotNavigate()
    {
        var (service, _, nav) = CreateService();

        await service.LoginAsync("wrong", "wrong");

        await nav.DidNotReceive().NavigateToDefaultAsync();
    }

    [Fact]
    public void Logout_ClearsAuthState()
    {
        var (service, authState, _) = CreateService();
        authState.SetAuthenticated("admin");

        service.Logout();

        authState.IsAuthenticated.Should().BeFalse();
        authState.Username.Should().BeNull();
    }
}
