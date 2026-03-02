using Microsoft.UI.Xaml.Controls;
using demos_applications_winui.Toolkit.Platform;

namespace demos_applications_winui.Toolkit.Toast;

/// <summary>
/// XAML control that renders the toast stack as a list of <see cref="InfoBar"/> controls.
/// Injected via DI and added to the visual tree programmatically by MainWindow.
/// </summary>
public sealed partial class ToastPresenter : UserControl
{
    private readonly IToastProvider _toastProvider;

    public IToastState ToastState { get; }

    public ToastPresenter(IToastState toastState, IToastProvider toastProvider)
    {
        _toastProvider = toastProvider;
        ToastState = toastState;

        InitializeComponent();
    }

    public static InfoBarSeverity ConvertSeverity(ToastSeverity severity) =>
        (InfoBarSeverity)(int)severity;

    private void Toast_Closed(InfoBar sender, InfoBarClosedEventArgs args)
    {
        if (sender.DataContext is ToastItem item)
        {
            _toastProvider.Dismiss(item.Id);
        }
    }
}
