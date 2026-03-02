using System;

namespace demos_applications_winui.Toolkit.Platform;

/// <summary>
/// Shows non-blocking toast notifications. Ephemeral toasts auto-dismiss after a delay;
/// persistent toasts remain until explicitly dismissed. Returns a <see cref="Guid"/>
/// that can be passed to <see cref="Dismiss"/> for early removal.
/// </summary>
public interface IToastProvider
{
    Guid Show(string title, string message, ToastSeverity severity = ToastSeverity.Informational, bool persistent = false);
    Guid ShowSuccess(string title, string message);
    Guid ShowError(string title, string message, bool persistent = false);
    void Dismiss(Guid toastId);
}
