using System.Collections.ObjectModel;

namespace demos_applications_winui.Toolkit.Toast;

/// <summary>
/// Read-only observable collection of active toasts. Bound by <see cref="ToastPresenter"/>
/// to render the toast stack in the visual tree.
/// </summary>
public interface IToastState
{
    ReadOnlyObservableCollection<ToastItem> Toasts { get; }
}
