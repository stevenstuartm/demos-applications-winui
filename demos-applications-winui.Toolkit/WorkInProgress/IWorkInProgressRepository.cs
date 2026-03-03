namespace demos_applications_winui.Toolkit.WorkInProgress;

/// <summary>
/// Central store for all WIP sessions. Singleton. Sessions are created on-demand
/// by key via <see cref="Get{TData}"/>. Domains inject this to manage editing
/// state that needs to survive transient VM disposal.
/// </summary>
public interface IWorkInProgressRepository
{
    /// <summary>
    /// Returns the session for the given key, creating it if it does not exist.
    /// Throws <see cref="System.InvalidOperationException"/> if the key exists
    /// but was created with a different <typeparamref name="TData"/> type.
    /// </summary>
    IWorkInProgress<TData> Get<TData>(string key) where TData : class;

    bool Has(string key);
    bool HasAnyPendingWork();
    void Remove(string key);
    void RemoveAll();
}
