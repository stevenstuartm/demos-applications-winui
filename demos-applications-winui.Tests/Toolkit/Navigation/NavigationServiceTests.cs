using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using demos_applications_winui.Core.Configuration;
using demos_applications_winui.Core.Navigation;
using demos_applications_winui.Toolkit.Navigation;
using Xunit;

namespace demos_applications_winui.Tests.Toolkit.Navigation;

public class NavigationServiceTests
{
    private class StubPage { }
    private class StubPageB { }
    private class RedirectTarget { }

    private class FakeFrame : INavigationFrame
    {
        public object? Content { get; set; }
    }

    private class NavigablePage : INavigable
    {
        public object? LastParameter { get; private set; }
        public bool InitializeCalled { get; private set; }
        public bool CanNavigateFrom { get; set; } = true;

        public Task InitializeAsync(object? parameter)
        {
            InitializeCalled = true;
            LastParameter = parameter;
            return Task.CompletedTask;
        }

        public Task<bool> CanNavigateFromAsync() => Task.FromResult(CanNavigateFrom);
    }

    private class DisposablePage : INavigable, IDisposable
    {
        public object? LastParameter { get; private set; }
        public bool Disposed { get; private set; }
        public bool CanNavigateFrom { get; set; } = true;

        public Task InitializeAsync(object? parameter)
        {
            LastParameter = parameter;
            return Task.CompletedTask;
        }

        public Task<bool> CanNavigateFromAsync() => Task.FromResult(CanNavigateFrom);
        public void Dispose() => Disposed = true;
    }

    private class BlockingNavigablePage : INavigable
    {
        private readonly Func<Task> _onInitialize;

        public BlockingNavigablePage(Func<Task> onInitialize)
        {
            _onInitialize = onInitialize;
        }

        public Task InitializeAsync(object? parameter) => _onInitialize();
    }

    private record ServiceContext(
        NavigationService Service,
        FakeFrame Frame,
        IServiceProvider ServiceProvider,
        NavigationState State,
        NavigationConfig Config);

    private static ServiceContext CreateService(
        IEnumerable<INavigationGuard>? guards = null,
        Action<NavigationConfig>? configureNav = null)
    {
        var frame = new FakeFrame();
        var state = new NavigationState();
        var config = new NavigationConfig { DefaultPage = typeof(StubPage) };
        configureNav?.Invoke(config);

        var sp = Substitute.For<IServiceProvider>();
        var guardList = guards ?? Array.Empty<INavigationGuard>();
        var service = new NavigationService(
            sp, guardList, state, config,
            NullLogger<NavigationService>.Instance);

        service.SetFrame(frame);
        return new ServiceContext(service, frame, sp, state, config);
    }

    // --- SetFrame & null-frame guard ---

    [Fact]
    public async Task NavigateToAsync_BeforeSetFrame_DoesNothing()
    {
        var sp = Substitute.For<IServiceProvider>();
        var state = new NavigationState();
        var config = new NavigationConfig { DefaultPage = typeof(StubPage) };
        var service = new NavigationService(
            sp, Array.Empty<INavigationGuard>(), state, config,
            NullLogger<NavigationService>.Instance);

        await service.NavigateToAsync<StubPage>();

        sp.DidNotReceive().GetService(Arg.Any<Type>());
        state.CurrentPageType.Should().BeNull();
    }

    [Fact]
    public async Task GoBackAsync_BeforeSetFrame_DoesNothing()
    {
        var sp = Substitute.For<IServiceProvider>();
        var state = new NavigationState();
        var config = new NavigationConfig { DefaultPage = typeof(StubPage) };
        var service = new NavigationService(
            sp, Array.Empty<INavigationGuard>(), state, config,
            NullLogger<NavigationService>.Instance);

        await service.GoBackAsync();

        sp.DidNotReceive().GetService(Arg.Any<Type>());
    }

    // --- Forward navigation (NavigateToAsync) ---

    [Fact]
    public async Task NavigateToAsync_ResolvesPageAndSetsContent()
    {
        var ctx = CreateService();
        var page = new StubPage();
        ctx.ServiceProvider.GetService(typeof(StubPage)).Returns(page);

        await ctx.Service.NavigateToAsync<StubPage>();

        ctx.Frame.Content.Should().BeSameAs(page);
    }

    [Fact]
    public async Task NavigateToAsync_CallsInitializeAsync()
    {
        var ctx = CreateService();
        var page = new NavigablePage();
        ctx.ServiceProvider.GetService(typeof(NavigablePage)).Returns(page);

        await ctx.Service.NavigateToAsync<NavigablePage>("param");

        page.InitializeCalled.Should().BeTrue();
        page.LastParameter.Should().Be("param");
    }

    [Fact]
    public async Task NavigateToAsync_PushesCurrentPageToBackStack()
    {
        var ctx = CreateService();
        var pageA = new StubPage();
        var pageB = new StubPageB();
        ctx.ServiceProvider.GetService(typeof(StubPage)).Returns(pageA);
        ctx.ServiceProvider.GetService(typeof(StubPageB)).Returns(pageB);

        await ctx.Service.NavigateToAsync<StubPage>("first");
        await ctx.Service.NavigateToAsync<StubPageB>("second");

        ctx.State.CanGoBack.Should().BeTrue();
        ctx.State.BackStack.Should().HaveCount(1);
        ctx.State.BackStack![0].PageType.Should().Be(typeof(StubPage));
        ctx.State.BackStack[0].Parameter.Should().Be("first");
    }

    [Fact]
    public async Task NavigateToAsync_UpdatesNavigationState()
    {
        var ctx = CreateService();
        var page = new StubPage();
        ctx.ServiceProvider.GetService(typeof(StubPage)).Returns(page);

        await ctx.Service.NavigateToAsync<StubPage>();

        ctx.State.CurrentPageType.Should().Be(typeof(StubPage));
        ctx.State.CanGoBack.Should().BeFalse();
    }

    [Fact]
    public async Task NavigateToAsync_DisposesCurrentPage()
    {
        var ctx = CreateService();
        var currentPage = new DisposablePage();
        var nextPage = new StubPageB();
        ctx.ServiceProvider.GetService(typeof(DisposablePage)).Returns(currentPage);
        ctx.ServiceProvider.GetService(typeof(StubPageB)).Returns(nextPage);

        await ctx.Service.NavigateToAsync<DisposablePage>();
        currentPage.Disposed.Should().BeFalse();

        await ctx.Service.NavigateToAsync<StubPageB>();

        currentPage.Disposed.Should().BeTrue();
    }

    // --- NavigateAndReplaceAsync ---

    [Fact]
    public async Task NavigateAndReplaceAsync_ClearsBackStack()
    {
        var ctx = CreateService();
        var pageA = new StubPage();
        var pageB = new StubPageB();
        ctx.ServiceProvider.GetService(typeof(StubPage)).Returns(pageA);
        ctx.ServiceProvider.GetService(typeof(StubPageB)).Returns(pageB);

        await ctx.Service.NavigateToAsync<StubPage>();
        await ctx.Service.NavigateAndReplaceAsync<StubPageB>();

        ctx.State.CanGoBack.Should().BeFalse();
        ctx.State.BackStack.Should().BeEmpty();
    }

    [Fact]
    public async Task NavigateAndReplaceAsync_CanGoBackIsFalse()
    {
        var ctx = CreateService();
        var page = new StubPage();
        ctx.ServiceProvider.GetService(typeof(StubPage)).Returns(page);

        await ctx.Service.NavigateAndReplaceAsync<StubPage>();

        ctx.State.CanGoBack.Should().BeFalse();
    }

    [Fact]
    public async Task NavigateAndReplaceAsync_DisposesCurrentPage()
    {
        var ctx = CreateService();
        var currentPage = new DisposablePage();
        var nextPage = new StubPageB();
        ctx.ServiceProvider.GetService(typeof(DisposablePage)).Returns(currentPage);
        ctx.ServiceProvider.GetService(typeof(StubPageB)).Returns(nextPage);

        await ctx.Service.NavigateToAsync<DisposablePage>();
        await ctx.Service.NavigateAndReplaceAsync<StubPageB>();

        currentPage.Disposed.Should().BeTrue();
    }

    [Fact]
    public async Task NavigateAndReplaceAsync_SkipsCanNavigateFrom()
    {
        var ctx = CreateService();
        var currentPage = new DisposablePage { CanNavigateFrom = false };
        var nextPage = new StubPageB();
        ctx.ServiceProvider.GetService(typeof(DisposablePage)).Returns(currentPage);
        ctx.ServiceProvider.GetService(typeof(StubPageB)).Returns(nextPage);

        await ctx.Service.NavigateToAsync<DisposablePage>();

        // NavigateAndReplaceAsync skips CanNavigateFromAsync (auth redirect pattern)
        await ctx.Service.NavigateAndReplaceAsync<StubPageB>();

        ctx.Frame.Content.Should().BeSameAs(nextPage);
        currentPage.Disposed.Should().BeTrue();
    }

    // --- NavigateToDefaultAsync ---

    [Fact]
    public async Task NavigateToDefaultAsync_NavigatesToConfiguredDefault()
    {
        var ctx = CreateService();
        var page = new StubPage();
        ctx.ServiceProvider.GetService(typeof(StubPage)).Returns(page);

        await ctx.Service.NavigateToDefaultAsync();

        ctx.State.CurrentPageType.Should().Be(typeof(StubPage));
        ctx.Frame.Content.Should().BeSameAs(page);
    }

    [Fact]
    public async Task NavigateToDefaultAsync_DisposesCurrentPage()
    {
        var ctx = CreateService();
        var currentPage = new DisposablePage();
        var defaultPage = new StubPage();
        ctx.ServiceProvider.GetService(typeof(DisposablePage)).Returns(currentPage);
        ctx.ServiceProvider.GetService(typeof(StubPage)).Returns(defaultPage);

        await ctx.Service.NavigateToAsync<DisposablePage>();
        await ctx.Service.NavigateToDefaultAsync();

        currentPage.Disposed.Should().BeTrue();
    }

    // --- GoBackAsync ---

    [Fact]
    public async Task GoBackAsync_CreatesFreshInstanceFromDI()
    {
        var ctx = CreateService();
        var pageA = new NavigablePage();
        var pageB = new StubPageB();
        ctx.ServiceProvider.GetService(typeof(NavigablePage)).Returns(pageA);
        ctx.ServiceProvider.GetService(typeof(StubPageB)).Returns(pageB);

        await ctx.Service.NavigateToAsync<NavigablePage>("param");
        await ctx.Service.NavigateToAsync<StubPageB>();

        // DI will return a fresh instance on back-nav
        var freshPageA = new NavigablePage();
        ctx.ServiceProvider.GetService(typeof(NavigablePage)).Returns(freshPageA);

        await ctx.Service.GoBackAsync();

        ctx.State.CurrentPageType.Should().Be(typeof(NavigablePage));
        ctx.Frame.Content.Should().BeSameAs(freshPageA);
        ctx.Frame.Content.Should().NotBeSameAs(pageA);
    }

    [Fact]
    public async Task GoBackAsync_EmptyBackStack_DoesNothing()
    {
        var ctx = CreateService();
        var page = new StubPage();
        ctx.ServiceProvider.GetService(typeof(StubPage)).Returns(page);
        await ctx.Service.NavigateToAsync<StubPage>();

        await ctx.Service.GoBackAsync();

        // Should still be on the same page
        ctx.State.CurrentPageType.Should().Be(typeof(StubPage));
        ctx.Frame.Content.Should().BeSameAs(page);
    }

    [Fact]
    public async Task GoBackAsync_DisposesCurrentPage()
    {
        var ctx = CreateService();
        var pageA = new StubPage();
        var pageB = new DisposablePage();
        ctx.ServiceProvider.GetService(typeof(StubPage)).Returns(pageA);
        ctx.ServiceProvider.GetService(typeof(DisposablePage)).Returns(pageB);

        await ctx.Service.NavigateToAsync<StubPage>();
        await ctx.Service.NavigateToAsync<DisposablePage>();

        pageB.Disposed.Should().BeFalse();

        await ctx.Service.GoBackAsync();

        pageB.Disposed.Should().BeTrue();
    }

    [Fact]
    public async Task GoBackAsync_RestoresParameter()
    {
        var ctx = CreateService();
        var pageA = new NavigablePage();
        var pageB = new StubPageB();
        ctx.ServiceProvider.GetService(typeof(NavigablePage)).Returns(pageA);
        ctx.ServiceProvider.GetService(typeof(StubPageB)).Returns(pageB);

        await ctx.Service.NavigateToAsync<NavigablePage>("original-param");
        await ctx.Service.NavigateToAsync<StubPageB>();

        var freshPageA = new NavigablePage();
        ctx.ServiceProvider.GetService(typeof(NavigablePage)).Returns(freshPageA);

        await ctx.Service.GoBackAsync();

        freshPageA.LastParameter.Should().Be("original-param");
    }

    [Fact]
    public async Task GoBackAsync_CallsInitializeAsyncOnFreshInstance()
    {
        var ctx = CreateService();
        var pageA = new NavigablePage();
        var pageB = new StubPageB();
        ctx.ServiceProvider.GetService(typeof(NavigablePage)).Returns(pageA);
        ctx.ServiceProvider.GetService(typeof(StubPageB)).Returns(pageB);

        await ctx.Service.NavigateToAsync<NavigablePage>("my-data");
        await ctx.Service.NavigateToAsync<StubPageB>();

        var freshPageA = new NavigablePage();
        ctx.ServiceProvider.GetService(typeof(NavigablePage)).Returns(freshPageA);

        await ctx.Service.GoBackAsync();

        freshPageA.InitializeCalled.Should().BeTrue();
        freshPageA.LastParameter.Should().Be("my-data");
    }

    // --- Guard evaluation ---

    [Fact]
    public async Task NavigateToAsync_GuardDenies_DoesNotNavigate()
    {
        var guard = Substitute.For<INavigationGuard>();
        guard.Name.Returns("TestGuard");
        guard.CheckNavigation(Arg.Any<NavigationGuardContext>())
            .Returns(new NavigationGuardResult { Allowed = false });

        var ctx = CreateService(
            guards: new[] { guard },
            configureNav: config =>
                config.Guard("TestGuard").ForPages(typeof(StubPage)));

        await ctx.Service.NavigateToAsync<StubPage>();

        ctx.ServiceProvider.DidNotReceive().GetService(typeof(StubPage));
        ctx.State.CurrentPageType.Should().BeNull();
    }

    [Fact]
    public async Task NavigateToAsync_GuardRedirects_NavigatesToRedirectTarget()
    {
        var guard = Substitute.For<INavigationGuard>();
        guard.Name.Returns("TestGuard");
        guard.CheckNavigation(Arg.Any<NavigationGuardContext>())
            .Returns(NavigationGuardResult.RedirectTo(typeof(RedirectTarget)));

        var redirectPage = new RedirectTarget();
        var ctx = CreateService(
            guards: new[] { guard },
            configureNav: config =>
                config.Guard("TestGuard").ForPages(typeof(StubPage)));
        ctx.ServiceProvider.GetService(typeof(RedirectTarget)).Returns(redirectPage);

        await ctx.Service.NavigateToAsync<StubPage>();

        ctx.State.CurrentPageType.Should().Be(typeof(RedirectTarget));
        ctx.Frame.Content.Should().BeSameAs(redirectPage);
    }

    [Fact]
    public async Task NavigateAndReplaceAsync_GuardRedirects_NavigatesToRedirectTarget()
    {
        var guard = Substitute.For<INavigationGuard>();
        guard.Name.Returns("TestGuard");
        guard.CheckNavigation(Arg.Any<NavigationGuardContext>())
            .Returns(NavigationGuardResult.RedirectTo(typeof(RedirectTarget)));

        var redirectPage = new RedirectTarget();
        var ctx = CreateService(
            guards: new[] { guard },
            configureNav: config =>
                config.Guard("TestGuard").ForPages(typeof(StubPage)));
        ctx.ServiceProvider.GetService(typeof(RedirectTarget)).Returns(redirectPage);

        await ctx.Service.NavigateAndReplaceAsync<StubPage>();

        ctx.State.CurrentPageType.Should().Be(typeof(RedirectTarget));
    }

    // --- CanNavigateFrom (outbound guard) ---

    [Fact]
    public async Task NavigateToAsync_CanNavigateFromReturnsFalse_CancelsNavigation()
    {
        var ctx = CreateService();
        var currentPage = new NavigablePage { CanNavigateFrom = false };
        var nextPage = new StubPageB();
        ctx.ServiceProvider.GetService(typeof(NavigablePage)).Returns(currentPage);
        ctx.ServiceProvider.GetService(typeof(StubPageB)).Returns(nextPage);

        await ctx.Service.NavigateToAsync<NavigablePage>();

        // Now try to navigate away — should be blocked by CanNavigateFromAsync
        await ctx.Service.NavigateToAsync<StubPageB>();

        ctx.State.CurrentPageType.Should().Be(typeof(NavigablePage));
        ctx.Frame.Content.Should().BeSameAs(currentPage);
    }

    [Fact]
    public async Task GoBackAsync_CanNavigateFromReturnsFalse_CancelsNavigation()
    {
        var ctx = CreateService();
        var pageA = new StubPage();
        var pageB = new NavigablePage { CanNavigateFrom = true };
        ctx.ServiceProvider.GetService(typeof(StubPage)).Returns(pageA);
        ctx.ServiceProvider.GetService(typeof(NavigablePage)).Returns(pageB);

        await ctx.Service.NavigateToAsync<StubPage>();
        await ctx.Service.NavigateToAsync<NavigablePage>();

        // Now block navigation away from pageB
        pageB.CanNavigateFrom = false;

        await ctx.Service.GoBackAsync();

        ctx.State.CurrentPageType.Should().Be(typeof(NavigablePage));
        ctx.Frame.Content.Should().BeSameAs(pageB);
    }

    [Fact]
    public async Task NavigateToAsync_CanNavigateFromReturnsFalse_DoesNotDispose()
    {
        var ctx = CreateService();
        var currentPage = new DisposablePage { CanNavigateFrom = false };
        var nextPage = new StubPageB();
        ctx.ServiceProvider.GetService(typeof(DisposablePage)).Returns(currentPage);
        ctx.ServiceProvider.GetService(typeof(StubPageB)).Returns(nextPage);

        await ctx.Service.NavigateToAsync<DisposablePage>();
        await ctx.Service.NavigateToAsync<StubPageB>();

        currentPage.Disposed.Should().BeFalse();
        ctx.Frame.Content.Should().BeSameAs(currentPage);
    }

    // --- Reentrancy protection ---

    [Fact]
    public async Task NavigateToAsync_WhileNavigationInProgress_IsIgnored()
    {
        var ctx = CreateService();
        var tcs = new TaskCompletionSource();

        var slowPage = new BlockingNavigablePage(() => tcs.Task);
        var nextPage = new StubPageB();
        ctx.ServiceProvider.GetService(typeof(BlockingNavigablePage)).Returns(slowPage);
        ctx.ServiceProvider.GetService(typeof(StubPageB)).Returns(nextPage);

        // Start a navigation that will block on InitializeAsync
        var firstNav = ctx.Service.NavigateToAsync<BlockingNavigablePage>();

        // Try to navigate while first is in progress — should be ignored
        await ctx.Service.NavigateToAsync<StubPageB>();

        // Complete the first navigation
        tcs.SetResult();
        await firstNav;

        ctx.State.CurrentPageType.Should().Be(typeof(BlockingNavigablePage));
        ctx.Frame.Content.Should().BeSameAs(slowPage);
    }

    // --- State management ---

    [Fact]
    public async Task NavigateToAsync_UpdatesCurrentPageType()
    {
        var ctx = CreateService();
        ctx.ServiceProvider.GetService(typeof(StubPage)).Returns(new StubPage());

        await ctx.Service.NavigateToAsync<StubPage>();

        ctx.State.CurrentPageType.Should().Be(typeof(StubPage));
    }

    [Fact]
    public async Task NavigateToAsync_MultipleNavigations_BackStackIsCorrect()
    {
        var ctx = CreateService();
        var pageA = new StubPage();
        var pageB = new StubPageB();
        var pageC = new NavigablePage();
        ctx.ServiceProvider.GetService(typeof(StubPage)).Returns(pageA);
        ctx.ServiceProvider.GetService(typeof(StubPageB)).Returns(pageB);
        ctx.ServiceProvider.GetService(typeof(NavigablePage)).Returns(pageC);

        await ctx.Service.NavigateToAsync<StubPage>("p1");
        await ctx.Service.NavigateToAsync<StubPageB>("p2");
        await ctx.Service.NavigateToAsync<NavigablePage>("p3");

        ctx.State.BackStack.Should().HaveCount(2);
        ctx.State.BackStack![0].PageType.Should().Be(typeof(StubPage));
        ctx.State.BackStack[0].Parameter.Should().Be("p1");
        ctx.State.BackStack[1].PageType.Should().Be(typeof(StubPageB));
        ctx.State.BackStack[1].Parameter.Should().Be("p2");
    }

    [Fact]
    public async Task NavigateToAsync_NoGuardsConfigured_NavigatesNormally()
    {
        var ctx = CreateService();
        var page = new StubPage();
        ctx.ServiceProvider.GetService(typeof(StubPage)).Returns(page);

        await ctx.Service.NavigateToAsync<StubPage>();

        ctx.State.CurrentPageType.Should().Be(typeof(StubPage));
        ctx.Frame.Content.Should().BeSameAs(page);
    }

    [Fact]
    public async Task GoBackAsync_UpdatesCanGoBackToFalse_WhenStackEmpty()
    {
        var ctx = CreateService();
        var pageA = new StubPage();
        var pageB = new StubPageB();
        ctx.ServiceProvider.GetService(typeof(StubPage)).Returns(pageA);
        ctx.ServiceProvider.GetService(typeof(StubPageB)).Returns(pageB);

        await ctx.Service.NavigateToAsync<StubPage>();
        await ctx.Service.NavigateToAsync<StubPageB>();
        ctx.State.CanGoBack.Should().BeTrue();

        await ctx.Service.GoBackAsync();
        ctx.State.CanGoBack.Should().BeFalse();
    }

    // --- Non-disposable pages are not affected ---

    [Fact]
    public async Task NavigateToAsync_NonDisposablePage_DoesNotThrow()
    {
        var ctx = CreateService();
        var pageA = new NavigablePage();
        var pageB = new StubPageB();
        ctx.ServiceProvider.GetService(typeof(NavigablePage)).Returns(pageA);
        ctx.ServiceProvider.GetService(typeof(StubPageB)).Returns(pageB);

        await ctx.Service.NavigateToAsync<NavigablePage>();

        var act = () => ctx.Service.NavigateToAsync<StubPageB>();

        await act.Should().NotThrowAsync();
    }
}
