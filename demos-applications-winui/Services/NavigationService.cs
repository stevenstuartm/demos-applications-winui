using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using demos_applications_winui.Core.Navigation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;

namespace demos_applications_winui.Services;

public partial class NavigationService(
    IServiceProvider serviceProvider,
    IEnumerable<INavigationGuard> guards) : ObservableObject, INavigationService
{
    private record NavigationEntry(Type PageType, object? Parameter);

    private Frame? _frame;
    private readonly Stack<NavigationEntry> _backStack = new();
    private object? _currentParameter;
    private bool _navigationInProgress;

    public bool CanGoBack => _backStack.Count > 0;

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
            OnPropertyChanged(nameof(CanGoBack));

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

    private (bool allowed, Type pageType, object? parameter) CheckGuards(
        Type pageType, object? parameter)
    {
        foreach (var guard in guards)
        {
            var result = guard.CheckNavigation(pageType);
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
                    _backStack.Push(new NavigationEntry(currentPage.GetType(), _currentParameter));
            }

            if (clearBackStack)
                _backStack.Clear();

            var page = (Page)serviceProvider.GetRequiredService(pageType);
            _frame.Content = page;
            _currentParameter = parameter;
            OnPropertyChanged(nameof(CanGoBack));

            if (page is INavigable navigable)
                _ = navigable.OnNavigatedToAsync(new NavigationContext(parameter, NavigationMode.New));
        }
        finally
        {
            _navigationInProgress = false;
        }
    }
}
