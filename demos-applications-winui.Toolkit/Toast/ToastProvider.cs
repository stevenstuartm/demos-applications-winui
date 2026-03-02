using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using demos_applications_winui.Toolkit.Platform;

namespace demos_applications_winui.Toolkit.Toast;

public partial class ToastProvider(ToastState toastState, ILogger<ToastProvider> logger) : IToastProvider
{
    private const int MaxVisible = 3;
    private const int AutoDismissDelayMs = 3000;

    private readonly Dictionary<Guid, CancellationTokenSource> _timers = new();

    public Guid Show(string title, string message, ToastSeverity severity = ToastSeverity.Informational, bool persistent = false)
    {
        EnforceCapacity();

        var item = new ToastItem
        {
            Id = Guid.NewGuid(),
            Title = title,
            Message = message,
            Severity = severity,
            IsPersistent = persistent
        };

        toastState.Add(item);
        LogToastShown(severity, title, message);

        if (!persistent)
        {
            StartAutoDismiss(item.Id);
        }

        return item.Id;
    }

    public Guid ShowSuccess(string title, string message) =>
        Show(title, message, ToastSeverity.Success);

    public Guid ShowError(string title, string message, bool persistent = false) =>
        Show(title, message, ToastSeverity.Error, persistent);

    public void Dismiss(Guid toastId)
    {
        CancelTimer(toastId);
        toastState.Remove(toastId);
    }

    private void EnforceCapacity()
    {
        while (toastState.Toasts.Count >= MaxVisible)
        {
            // Evict oldest ephemeral first, then oldest persistent
            var victim = toastState.Toasts.FirstOrDefault(t => !t.IsPersistent)
                         ?? toastState.Toasts[0];

            Dismiss(victim.Id);
        }
    }

    private void StartAutoDismiss(Guid toastId)
    {
        var cts = new CancellationTokenSource();
        _timers[toastId] = cts;

        _ = DismissAfterDelayAsync(toastId, cts.Token);
    }

    private async Task DismissAfterDelayAsync(Guid toastId, CancellationToken token)
    {
        try
        {
            await Task.Delay(AutoDismissDelayMs, token);
            Dismiss(toastId);
        }
        catch (OperationCanceledException)
        {
            // Expected when manually dismissed before timer fires
        }
    }

    private void CancelTimer(Guid toastId)
    {
        if (_timers.Remove(toastId, out var cts))
        {
            cts.Cancel();
            cts.Dispose();
        }
    }

    [LoggerMessage(EventId = 3000, Level = LogLevel.Debug, Message = "Toast shown: [{Severity}] {Title} - {Message}")]
    partial void LogToastShown(ToastSeverity severity, string title, string message);
}
