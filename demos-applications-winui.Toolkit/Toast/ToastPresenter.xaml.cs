using Microsoft.UI.Xaml.Controls;
using demos_applications_winui.Core.Platform;

namespace demos_applications_winui.Toolkit.Toast;

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
