using System.Collections.ObjectModel;

namespace demos_applications_winui.Toolkit.Toast;

public interface IToastState
{
    ReadOnlyObservableCollection<ToastItem> Toasts { get; }
}
