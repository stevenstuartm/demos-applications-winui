using System;
using FluentAssertions;
using demos_applications_winui.Core.Navigation;
using Xunit;

namespace demos_applications_winui.Tests.Core.Navigation;

public class NavigationGuardResultTests
{
    [Fact]
    public void Allow_ReturnsAllowedTrue()
    {
        var result = NavigationGuardResult.Allow;

        result.Allowed.Should().BeTrue();
        result.RedirectPageType.Should().BeNull();
    }

    [Fact]
    public void RedirectTo_ReturnsAllowedFalse_WithRedirectType()
    {
        var result = NavigationGuardResult.RedirectTo(typeof(string));

        result.Allowed.Should().BeFalse();
        result.RedirectPageType.Should().Be(typeof(string));
    }

    [Fact]
    public void Allow_IsSingleton_ReturnsSameInstance()
    {
        var first = NavigationGuardResult.Allow;
        var second = NavigationGuardResult.Allow;

        first.Should().BeSameAs(second);
    }
}
