using System;
using System.Collections.Generic;
using System.Linq;
using demos_applications_winui.Core.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;

namespace demos_applications_winui.Core.Navigation;

public class NavigationService(
    IServiceProvider serviceProvider,
    IEnumerable<INavigationGuard> guards,
    NavigationState navigationState,
    AppConfig config) : INavigationService
{
    private readonly Type _defaultPage = config.Navigation.DefaultPage
        ?? throw new InvalidOperationException(
            "NavigationConfig.DefaultPage must be configured via AddAppConfig.");

    private Frame? _frame;
    private readonly Stack<NavigationStackEntry> _backStack = new();
    private object? _currentParameter;
    private bool _navigationInProgress;

    public void SetFrame(Frame frame) => _frame = frame;

    public void GoBack()
    {
        if (_frame is null || _backStack.Count == 0 || _navigationInProgress) return;

        _navigationInProgress = true;

        try
        {
            if (_frame.Content is INavigable currentNavigable)
                currentNavigable.OnNavigatedFrom();

            var entry = _backStack.Pop();
            var page = (Page)serviceProvider.GetRequiredService(entry.PageType);
            _frame.Content = page;
            _currentParameter = entry.Parameter;
            UpdateNavigationState(entry.PageType);

            if (page is INavigable navigable)
                _ = navigable.OnNavigatedToAsync(new NavigationContext(entry.Parameter, NavigationMode.Back));
        }
        finally
        {
            _navigationInProgress = false;
        }
    }

    public void NavigateTo<TPage>(object? parameter = null) where TPage : Page
    {
        if (_frame is null || _navigationInProgress) return;

        var (allowed, targetType, targetParam) = CheckGuards(typeof(TPage), parameter);
        if (!allowed) return;

        var isRedirect = targetType != typeof(TPage);
        NavigateCore(targetType, targetParam, clearBackStack: isRedirect);
    }

    public void NavigateAndReplace<TPage>(object? parameter = null) where TPage : Page
    {
        if (_frame is null || _navigationInProgress) return;

        var (allowed, targetType, targetParam) = CheckGuards(typeof(TPage), parameter);
        if (!allowed) return;

        NavigateCore(targetType, targetParam, clearBackStack: true);
    }

    public void NavigateToDefault()
    {
        if (_frame is null || _navigationInProgress) return;

        var (allowed, targetType, targetParam) = CheckGuards(_defaultPage, null);
        if (!allowed) return;

        NavigateCore(targetType, targetParam, clearBackStack: true);
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
        var requestedGuards = config.Navigation.GetGuardsForPage(pageType);

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

    private void NavigateCore(Type pageType, object? parameter, bool clearBackStack)
    {
        _navigationInProgress = true;

        try
        {
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
                _ = navigable.OnNavigatedToAsync(new NavigationContext(parameter, NavigationMode.New));
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
}
