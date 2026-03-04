using System;
using System.Threading.Tasks;

namespace demos_applications_winui.Toolkit.Navigation;

/// <summary>
/// Custom navigation service that bypasses <c>Frame.Navigate</c> in favor of
/// DI-resolved transient pages set as <c>Frame.Content</c>. Each navigation creates
/// a fresh page instance, disposes the previous one, and calls <see cref="INavigable.InitializeAsync"/>.
/// Supports guard evaluation and back-stack management.
/// </summary>
public interface INavigationService
{
    Task GoBackAsync();
    Task NavigateToAsync<TPage>(object? parameter = null);
    Task NavigateToAsync(Type pageType, object? parameter = null);

    /// <summary>
    /// Navigates to the target page and clears the back stack, preventing back-navigation
    /// to previous pages. Used for flows like post-login redirect.
    /// </summary>
    Task NavigateAndReplaceAsync<TPage>(object? parameter = null);

    /// <summary>
    /// Navigates to the page configured as <see cref="Core.Configuration.NavigationConfig.DefaultPage"/>,
    /// clearing the back stack. Typically called once at startup after config providers finish.
    /// </summary>
    Task NavigateToDefaultAsync();

    /// <summary>
    /// Binds this service to the content frame. Must be called once from MainWindow
    /// before any navigation occurs.
    /// </summary>
    void SetFrame(INavigationFrame frame);
}
