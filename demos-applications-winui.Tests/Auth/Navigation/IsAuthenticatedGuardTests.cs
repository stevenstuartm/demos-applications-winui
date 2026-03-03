using System;
using FluentAssertions;
using NSubstitute;
using demos_applications_winui.Auth.Navigation;
using demos_applications_winui.Auth.Views;
using demos_applications_winui.Core.Auth;
using demos_applications_winui.Core.Navigation;
using Xunit;

namespace demos_applications_winui.Tests.Auth.Navigation;

public class IsAuthenticatedGuardTests
{
    private static (IsAuthenticatedGuard guard, IAuthState authState) CreateGuard()
    {
        var authState = Substitute.For<IAuthState>();
        var guard = new IsAuthenticatedGuard(authState);
        return (guard, authState);
    }

    [Fact]
    public void Name_ReturnsIsAuthenticatedConstant()
    {
        var (guard, _) = CreateGuard();

        guard.Name.Should().Be(NavigationGuardNames.IsAuthenticated);
    }

    [Fact]
    public void CheckNavigation_WhenAuthenticated_ReturnsAllow()
    {
        var (guard, authState) = CreateGuard();
        authState.IsAuthenticated.Returns(true);
        var context = new NavigationGuardContext(typeof(object), null, null);

        var result = guard.CheckNavigation(context);

        result.Allowed.Should().BeTrue();
    }

    [Fact]
    public void CheckNavigation_WhenNotAuthenticated_RedirectsToLoginPage()
    {
        var (guard, authState) = CreateGuard();
        authState.IsAuthenticated.Returns(false);
        var context = new NavigationGuardContext(typeof(object), null, null);

        var result = guard.CheckNavigation(context);

        result.Allowed.Should().BeFalse();
        result.RedirectPageType.Should().Be(typeof(LoginPage));
    }
}
