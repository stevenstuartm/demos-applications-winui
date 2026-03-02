using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace demos_applications_winui.Toolkit.Toast;

/// <summary>
/// Mutable backing store for active toasts. Mutations are <c>internal</c> —
/// only <see cref="ToastProvider"/> adds and removes items.
/// </summary>
public class ToastState : IToastState
{
    private readonly ObservableCollection<ToastItem> _toasts = new();

    public ReadOnlyObservableCollection<ToastItem> Toasts { get; }

    public ToastState()
    {
        Toasts = new ReadOnlyObservableCollection<ToastItem>(_toasts);
    }

    internal void Add(ToastItem item) => _toasts.Add(item);

    internal bool Remove(Guid id)
    {
        var item = _toasts.FirstOrDefault(t => t.Id == id);
        if (item is null) return false;
        return _toasts.Remove(item);
    }
}
