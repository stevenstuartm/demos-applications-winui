using System.Collections.Generic;
using FluentAssertions;
using demos_applications_winui.Auth.Models;
using Xunit;

namespace demos_applications_winui.Tests.Auth.Models;

public class AuthStateTests
{
    private static AuthState CreateState() => new();

    [Fact]
    public void Initial_IsNotAuthenticated()
    {
        var state = CreateState();

        state.IsAuthenticated.Should().BeFalse();
        state.Username.Should().BeNull();
    }

    [Fact]
    public void SetAuthenticated_SetsUsernameAndFlag()
    {
        var state = CreateState();

        state.SetAuthenticated("testuser");

        state.IsAuthenticated.Should().BeTrue();
        state.Username.Should().Be("testuser");
    }

    [Fact]
    public void Clear_ResetsToInitialState()
    {
        var state = CreateState();
        state.SetAuthenticated("testuser");

        state.Clear();

        state.IsAuthenticated.Should().BeFalse();
        state.Username.Should().BeNull();
    }

    [Fact]
    public void SetAuthenticated_RaisesPropertyChanged()
    {
        var state = CreateState();
        var changedProperties = new List<string>();
        state.PropertyChanged += (_, e) => changedProperties.Add(e.PropertyName!);

        state.SetAuthenticated("user");

        changedProperties.Should().Contain(nameof(AuthState.Username));
        changedProperties.Should().Contain(nameof(AuthState.IsAuthenticated));
    }

    [Fact]
    public void Clear_RaisesPropertyChanged()
    {
        var state = CreateState();
        state.SetAuthenticated("user");
        var changedProperties = new List<string>();
        state.PropertyChanged += (_, e) => changedProperties.Add(e.PropertyName!);

        state.Clear();

        changedProperties.Should().Contain(nameof(AuthState.Username));
        changedProperties.Should().Contain(nameof(AuthState.IsAuthenticated));
    }
}
