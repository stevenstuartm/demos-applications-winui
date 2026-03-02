using System;

namespace demos_applications_winui.Toolkit.Platform;

public interface IToastProvider
{
    Guid Show(string title, string message, ToastSeverity severity = ToastSeverity.Informational, bool persistent = false);
    Guid ShowSuccess(string title, string message);
    Guid ShowError(string title, string message, bool persistent = false);
    void Dismiss(Guid toastId);
}
