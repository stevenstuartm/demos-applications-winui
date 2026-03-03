using System.ComponentModel;

namespace demos_applications_winui.Toolkit.WorkInProgress;

/// <summary>
/// Non-generic base for WIP sessions. Used by the repository for cross-cutting
/// queries (e.g., "does any session have pending work?").
/// </summary>
public interface IWorkInProgress : INotifyPropertyChanged
{
    bool HasPendingWork { get; }
    void Clear();
}

/// <summary>
/// Typed WIP session that holds domain-specific editing data. VMs interact
/// with this interface — the repository creates and manages session instances.
/// </summary>
public interface IWorkInProgress<TData> : IWorkInProgress where TData : class
{
    TData? Data { get; }
    bool IsDirty { get; }
    void Begin(TData data);
    void MarkDirty();
    void MarkClean();
}
