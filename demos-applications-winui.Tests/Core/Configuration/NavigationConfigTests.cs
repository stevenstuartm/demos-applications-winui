using System;
using FluentAssertions;
using demos_applications_winui.Core.Configuration;
using Xunit;

namespace demos_applications_winui.Tests.Core.Configuration;

public class NavigationConfigTests
{
    private static NavigationConfig CreateConfig() =>
        new() { DefaultPage = typeof(StubPageA) };

    [Fact]
    public void GetGuardsForPage_NoGuardsConfigured_ReturnsEmptySet()
    {
        var config = CreateConfig();

        var guards = config.GetGuardsForPage(typeof(StubPageA));

        guards.Should().BeEmpty();
    }

    [Fact]
    public void Guard_ForPages_AppliesGuardToSpecifiedPages()
    {
        var config = CreateConfig();
        config.Guard("TestGuard").ForPages(typeof(StubPageA), typeof(StubPageB));

        config.GetGuardsForPage(typeof(StubPageA)).Should().Contain("TestGuard");
        config.GetGuardsForPage(typeof(StubPageB)).Should().Contain("TestGuard");
        config.GetGuardsForPage(typeof(StubPageC)).Should().NotContain("TestGuard");
    }

    [Fact]
    public void Guard_ForAll_AppliesGuardToEveryPage()
    {
        var config = CreateConfig();
        config.Guard("GlobalGuard").ForAll();

        config.GetGuardsForPage(typeof(StubPageA)).Should().Contain("GlobalGuard");
        config.GetGuardsForPage(typeof(StubPageB)).Should().Contain("GlobalGuard");
        config.GetGuardsForPage(typeof(StubPageC)).Should().Contain("GlobalGuard");
    }

    [Fact]
    public void Guard_ForAll_Except_ExcludesSpecifiedPages()
    {
        var config = CreateConfig();
        config.Guard("GlobalGuard").ForAll().Except(typeof(StubPageB));

        config.GetGuardsForPage(typeof(StubPageA)).Should().Contain("GlobalGuard");
        config.GetGuardsForPage(typeof(StubPageB)).Should().NotContain("GlobalGuard");
        config.GetGuardsForPage(typeof(StubPageC)).Should().Contain("GlobalGuard");
    }

    [Fact]
    public void GetGuardsForPage_MultipleGuards_ReturnsAll()
    {
        var config = CreateConfig();
        config
            .Guard("Guard1").ForPages(typeof(StubPageA))
            .Guard("Guard2").ForAll().Except(typeof(StubPageC));

        var guards = config.GetGuardsForPage(typeof(StubPageA));

        guards.Should().Contain("Guard1");
        guards.Should().Contain("Guard2");
    }

    [Fact]
    public void GetGuardsForPage_CachesResult_ReturnsSameInstance()
    {
        var config = CreateConfig();
        config.Guard("CachedGuard").ForPages(typeof(StubPageA));

        var first = config.GetGuardsForPage(typeof(StubPageA));
        var second = config.GetGuardsForPage(typeof(StubPageA));

        first.Should().BeSameAs(second);
    }

    [Fact]
    public void Guard_ForAll_Except_MultipleExclusions()
    {
        var config = CreateConfig();
        config.Guard("Strict").ForAll().Except(typeof(StubPageA), typeof(StubPageC));

        config.GetGuardsForPage(typeof(StubPageA)).Should().NotContain("Strict");
        config.GetGuardsForPage(typeof(StubPageB)).Should().Contain("Strict");
        config.GetGuardsForPage(typeof(StubPageC)).Should().NotContain("Strict");
    }

    [Fact]
    public void DefaultPage_IsRequired()
    {
        var config = new NavigationConfig { DefaultPage = typeof(StubPageA) };

        config.DefaultPage.Should().Be(typeof(StubPageA));
    }
}

// Stub types for testing guard routing — these don't need to be real Pages
// since NavigationConfig only stores Type references
public class StubPageA;
public class StubPageB;
public class StubPageC;
