using System;
using demos_applications_winui.Core.Platform;

namespace demos_applications_winui.Toolkit.Toast;

public class ToastItem
{
    public required Guid Id { get; init; }
    public required string Title { get; init; }
    public required string Message { get; init; }
    public ToastSeverity Severity { get; init; }
    public bool IsPersistent { get; init; }
}
