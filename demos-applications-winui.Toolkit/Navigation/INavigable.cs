using System.Threading.Tasks;

namespace demos_applications_winui.Toolkit.Navigation;

/// <summary>
/// Optional lifecycle contract for pages participating in the navigation system.
/// Pages are transient — each navigation creates a fresh instance.
/// <see cref="InitializeAsync"/> is called once after creation with the navigation parameter.
/// <see cref="CanNavigateFromAsync"/> is called before leaving to allow cancellation
/// (e.g., unsaved changes prompt).
/// </summary>
public interface INavigable
{
    /// <summary>
    /// Called once after the page is created and set as content. Receives the
    /// navigation parameter (e.g., a Note to edit). Default is no-op.
    /// </summary>
    Task InitializeAsync(object? parameter) => Task.CompletedTask;

    /// <summary>
    /// Async outbound guard — returning <c>false</c> cancels the pending navigation.
    /// Override to prompt for unsaved changes confirmation before leaving.
    /// </summary>
    Task<bool> CanNavigateFromAsync() => Task.FromResult(true);
}
