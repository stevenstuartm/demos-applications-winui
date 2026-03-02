using System.Threading;
using System.Threading.Tasks;

namespace demos_applications_winui.Core.Configuration;

/// <summary>
/// Contract for async config providers that load remote configuration during app startup,
/// before first navigation. Implementations should be resilient: cache last-known-good values,
/// timeout gracefully, and never throw — log warnings on failure and fall back to defaults.
/// </summary>
public interface IStartupConfigProvider
{
    /// <summary>
    /// Priority order. Lower values run first.
    /// </summary>
    int Order => 0;

    Task LoadAsync(CancellationToken cancellationToken = default);
}
