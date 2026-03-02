using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;

namespace demos_applications_winui.Toolkit.Navigation;

/// <summary>
/// Custom navigation service that bypasses <c>Frame.Navigate</c> in favor of
/// DI-resolved singleton pages set as <c>Frame.Content</c>. Supports guard
/// evaluation, back-stack management, and <see cref="INavigable"/> lifecycle callbacks.
/// </summary>
public interface INavigationService
{
    Task GoBackAsync();
    Task NavigateToAsync<TPage>(object? parameter = null) where TPage : Page;

    /// <summary>
    /// Navigates to the target page and clears the back stack, preventing back-navigation
    /// to previous pages. Used for flows like post-login redirect.
    /// </summary>
    Task NavigateAndReplaceAsync<TPage>(object? parameter = null) where TPage : Page;

    /// <summary>
    /// Navigates to the page configured as <see cref="Core.Configuration.NavigationConfig.DefaultPage"/>,
    /// clearing the back stack. Typically called once at startup after config providers finish.
    /// </summary>
    Task NavigateToDefaultAsync();

    /// <summary>
    /// Binds this service to the content frame. Must be called once from MainWindow
    /// before any navigation occurs.
    /// </summary>
    void SetFrame(Frame frame);
}
