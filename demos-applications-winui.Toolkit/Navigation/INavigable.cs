using System.Threading.Tasks;

namespace demos_applications_winui.Toolkit.Navigation;

/// <summary>
/// Lifecycle contract implemented by pages (and delegated to their view models).
/// <see cref="NavigationService"/> calls these methods at each stage of navigation.
/// </summary>
public interface INavigable
{
    /// <summary>
    /// Called after the page becomes the active content. Use for loading data,
    /// restoring editor state, or reacting to the navigation parameter.
    /// </summary>
    Task OnNavigatedToAsync(NavigationContext context);

    /// <summary>
    /// Called when the page is being replaced. Use for cleanup, cancelling
    /// in-flight commands, or clearing sensitive fields.
    /// </summary>
    void OnNavigatedFrom();

    /// <summary>
    /// Async outbound guard — returning <c>false</c> cancels the pending navigation.
    /// Override to prompt for unsaved changes confirmation before leaving.
    /// </summary>
    Task<bool> CanNavigateFromAsync() => Task.FromResult(true);
}
