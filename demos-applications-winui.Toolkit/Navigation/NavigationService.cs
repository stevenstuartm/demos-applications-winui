using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using demos_applications_winui.Core.Configuration;
using demos_applications_winui.Core.Navigation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml.Controls;

namespace demos_applications_winui.Toolkit.Navigation;

public partial class NavigationService(
    IServiceProvider serviceProvider,
    IEnumerable<INavigationGuard> guards,
    NavigationState navigationState,
    NavigationConfig navigationConfig,
    ILogger<NavigationService> logger) : INavigationService
{
    private Frame? _frame;
    private readonly Stack<NavigationStackEntry> _backStack = new();
    private object? _currentParameter;
    private bool _navigationInProgress;

    public void SetFrame(Frame frame) => _frame = frame;

    public async Task GoBackAsync()
    {
        if (_frame is null || _backStack.Count == 0 || _navigationInProgress) return;

        if (!await CheckCanNavigateFromAsync())
            return;

        _navigationInProgress = true;

        try
        {
            LogNavigatingBack();

            if (_frame.Content is INavigable currentNavigable)
                currentNavigable.OnNavigatedFrom();

            var entry = _backStack.Pop();
            var page = (Page)serviceProvider.GetRequiredService(entry.PageType);
            _frame.Content = page;
            _currentParameter = entry.Parameter;
            UpdateNavigationState(entry.PageType);

            if (page is INavigable navigable)
                await navigable.OnNavigatedToAsync(new NavigationContext(entry.Parameter, NavigationMode.Back));

            LogNavigatedBackTo(entry.PageType.Name);
        }
        catch (Exception ex)
        {
            LogGoBackFailed(ex);
            throw;
        }
        finally
        {
            _navigationInProgress = false;
        }
    }

    public async Task NavigateToAsync<TPage>(object? parameter = null) where TPage : Page
    {
        if (_frame is null || _navigationInProgress) return;

        if (!await CheckCanNavigateFromAsync())
            return;

        var (allowed, targetType, targetParam) = CheckGuards(typeof(TPage), parameter);
        if (!allowed) return;

        var isRedirect = targetType != typeof(TPage);
        await NavigateCoreAsync(targetType, targetParam, clearBackStack: isRedirect);
    }

    public async Task NavigateAndReplaceAsync<TPage>(object? parameter = null) where TPage : Page
    {
        if (_frame is null || _navigationInProgress) return;

        var (allowed, targetType, targetParam) = CheckGuards(typeof(TPage), parameter);
        if (!allowed) return;

        await NavigateCoreAsync(targetType, targetParam, clearBackStack: true);
    }

    public async Task NavigateToDefaultAsync()
    {
        if (_frame is null || _navigationInProgress) return;

        var (allowed, targetType, targetParam) = CheckGuards(navigationConfig.DefaultPage, null);
        if (!allowed) return;

        await NavigateCoreAsync(targetType, targetParam, clearBackStack: true);
    }

    private async Task<bool> CheckCanNavigateFromAsync()
    {
        if (_frame?.Content is INavigable currentNavigable)
            return await currentNavigable.CanNavigateFromAsync();

        return true;
    }

    /// <summary>
    /// Determines which guards apply to the target page using the guard routing
    /// configured in NavigationConfig. Guard-to-page mappings are resolved from
    /// startup configuration (no reflection) and cached per page type.
    /// Only guards whose Name matches a configured mapping are evaluated.
    /// </summary>
    private (bool allowed, Type pageType, object? parameter) CheckGuards(
        Type pageType, object? parameter)
    {
        var requestedGuards = navigationConfig.GetGuardsForPage(pageType);

        if (requestedGuards.Count == 0)
            return (true, pageType, parameter);

        var context = new NavigationGuardContext(pageType, navigationState.CurrentPageType, parameter);

        foreach (var guard in guards)
        {
            if (!requestedGuards.Contains(guard.Name))
                continue;

            var result = guard.CheckNavigation(context);
            if (!result.Allowed)
            {
                return result.RedirectPageType is not null
                    ? (true, result.RedirectPageType, null)
                    : (false, pageType, parameter);
            }
        }

        return (true, pageType, parameter);
    }

    private async Task NavigateCoreAsync(Type pageType, object? parameter, bool clearBackStack)
    {
        _navigationInProgress = true;

        try
        {
            LogNavigatingTo(pageType.Name, clearBackStack);

            if (_frame!.Content is Page currentPage)
            {
                if (currentPage is INavigable currentNavigable)
                    currentNavigable.OnNavigatedFrom();

                if (!clearBackStack)
                    _backStack.Push(new NavigationStackEntry(currentPage.GetType(), _currentParameter));
            }

            if (clearBackStack)
                _backStack.Clear();

            var page = (Page)serviceProvider.GetRequiredService(pageType);
            _frame.Content = page;
            _currentParameter = parameter;
            UpdateNavigationState(pageType);

            if (page is INavigable navigable)
                await navigable.OnNavigatedToAsync(new NavigationContext(parameter, NavigationMode.New));

            LogNavigatedTo(pageType.Name);
        }
        catch (Exception ex)
        {
            LogNavigationFailed(ex, pageType.Name);
            throw;
        }
        finally
        {
            _navigationInProgress = false;
        }
    }

    private void UpdateNavigationState(Type currentPageType)
    {
        navigationState.CanGoBack = _backStack.Count > 0;
        navigationState.CurrentPageType = currentPageType;
        navigationState.BackStack = _backStack.Reverse().ToList();
    }

    [LoggerMessage(EventId = 1000, Level = LogLevel.Debug, Message = "Navigating back")]
    partial void LogNavigatingBack();

    [LoggerMessage(EventId = 1001, Level = LogLevel.Debug, Message = "Navigated back to {PageType}")]
    partial void LogNavigatedBackTo(string pageType);

    [LoggerMessage(EventId = 1002, Level = LogLevel.Error, Message = "GoBack navigation failed")]
    partial void LogGoBackFailed(Exception ex);

    [LoggerMessage(EventId = 1003, Level = LogLevel.Debug, Message = "Navigating to {PageType}, clearBackStack={ClearBackStack}")]
    partial void LogNavigatingTo(string pageType, bool clearBackStack);

    [LoggerMessage(EventId = 1004, Level = LogLevel.Information, Message = "Navigated to {PageType}")]
    partial void LogNavigatedTo(string pageType);

    [LoggerMessage(EventId = 1005, Level = LogLevel.Error, Message = "Navigation to {PageType} failed")]
    partial void LogNavigationFailed(Exception ex, string pageType);
}
