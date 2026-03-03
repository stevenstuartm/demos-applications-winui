using CommunityToolkit.Mvvm.ComponentModel;

namespace demos_applications_winui.Toolkit.WorkInProgress;

/// <summary>
/// Holds editing state for a single WIP session. Internal — only the
/// <see cref="WorkInProgressRepository"/> creates these. VMs interact
/// via <see cref="IWorkInProgress{TData}"/>.
/// </summary>
internal partial class WorkInProgress<TData> : ObservableObject, IWorkInProgress<TData>
    where TData : class
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasPendingWork))]
    public partial TData? Data { get; private set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasPendingWork))]
    public partial bool IsDirty { get; private set; }

    public bool HasPendingWork => Data is not null && IsDirty;

    public void Begin(TData data)
    {
        Data = data;
        IsDirty = false;
    }

    public void MarkDirty() => IsDirty = true;

    public void MarkClean() => IsDirty = false;

    public void Clear()
    {
        Data = default;
        IsDirty = false;
    }
}
