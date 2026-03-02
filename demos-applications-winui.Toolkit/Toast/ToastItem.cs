using System;
using demos_applications_winui.Toolkit.Platform;

namespace demos_applications_winui.Toolkit.Toast;

/// <summary>
/// Immutable data for a single toast notification. <see cref="IsPersistent"/> toasts
/// remain visible until the user or code explicitly dismisses them.
/// </summary>
public class ToastItem
{
    public required Guid Id { get; init; }
    public required string Title { get; init; }
    public required string Message { get; init; }
    public ToastSeverity Severity { get; init; }
    public bool IsPersistent { get; init; }
}
