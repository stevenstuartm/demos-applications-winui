using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using demos_applications_winui.Auth.Models;
using demos_applications_winui.Auth.Services;
using demos_applications_winui.Auth.ViewModels;
using demos_applications_winui.Toolkit.Navigation;
using Xunit;

namespace demos_applications_winui.Tests.Auth.ViewModels;

public class LoginViewModelTests
{
    private static (LoginViewModel vm, IAuthService authService) CreateVm()
    {
        var authService = Substitute.For<IAuthService>();
        var vm = new LoginViewModel(authService);
        return (vm, authService);
    }

    [Fact]
    public void CanLogin_BothEmpty_ReturnsFalse()
    {
        var (vm, _) = CreateVm();
        vm.Username = null;
        vm.Password = null;

        vm.LoginCommand.CanExecute(null).Should().BeFalse();
    }

    [Fact]
    public void CanLogin_UsernameOnly_ReturnsFalse()
    {
        var (vm, _) = CreateVm();
        vm.Username = "admin";
        vm.Password = null;

        vm.LoginCommand.CanExecute(null).Should().BeFalse();
    }

    [Fact]
    public void CanLogin_PasswordOnly_ReturnsFalse()
    {
        var (vm, _) = CreateVm();
        vm.Username = null;
        vm.Password = "pass";

        vm.LoginCommand.CanExecute(null).Should().BeFalse();
    }

    [Fact]
    public void CanLogin_BothProvided_ReturnsTrue()
    {
        var (vm, _) = CreateVm();
        vm.Username = "admin";
        vm.Password = "pass";

        vm.LoginCommand.CanExecute(null).Should().BeTrue();
    }

    [Fact]
    public void CanLogin_WhitespaceUsername_ReturnsFalse()
    {
        var (vm, _) = CreateVm();
        vm.Username = "   ";
        vm.Password = "pass";

        vm.LoginCommand.CanExecute(null).Should().BeFalse();
    }

    [Fact]
    public async Task LoginAsync_Success_ClearsError()
    {
        var (vm, authService) = CreateVm();
        vm.Username = "admin";
        vm.Password = "password";
        authService.LoginAsync("admin", "password")
            .Returns(new AuthResult(true));

        await vm.LoginCommand.ExecuteAsync(null);

        vm.HasError.Should().BeFalse();
        vm.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public async Task LoginAsync_Failure_SetsError()
    {
        var (vm, authService) = CreateVm();
        vm.Username = "admin";
        vm.Password = "wrong";
        authService.LoginAsync("admin", "wrong")
            .Returns(new AuthResult(false, "Invalid credentials"));

        await vm.LoginCommand.ExecuteAsync(null);

        vm.HasError.Should().BeTrue();
        vm.ErrorMessage.Should().Be("Invalid credentials");
    }

    [Fact]
    public async Task OnNavigatedToAsync_ClearsAllState()
    {
        var (vm, _) = CreateVm();
        vm.Username = "leftover";
        vm.Password = "leftover";
        vm.ErrorMessage = "old error";
        vm.HasError = true;

        await vm.OnNavigatedToAsync(new NavigationContext(null, NavigationMode.New));

        vm.Username.Should().BeNull();
        vm.Password.Should().BeNull();
        vm.ErrorMessage.Should().BeNull();
        vm.HasError.Should().BeFalse();
    }

    [Fact]
    public void OnNavigatedFrom_ClearsAllState()
    {
        var (vm, _) = CreateVm();
        vm.Username = "leftover";
        vm.Password = "leftover";
        vm.ErrorMessage = "old error";
        vm.HasError = true;

        vm.OnNavigatedFrom();

        vm.Username.Should().BeNull();
        vm.Password.Should().BeNull();
        vm.ErrorMessage.Should().BeNull();
        vm.HasError.Should().BeFalse();
    }

    [Fact]
    public async Task LoginAsync_ClearsPreviousError_BeforeAttempt()
    {
        var (vm, authService) = CreateVm();
        vm.ErrorMessage = "previous error";
        vm.HasError = true;
        vm.Username = "admin";
        vm.Password = "password";
        authService.LoginAsync("admin", "password")
            .Returns(new AuthResult(true));

        await vm.LoginCommand.ExecuteAsync(null);

        vm.HasError.Should().BeFalse();
        vm.ErrorMessage.Should().BeNull();
    }
}
