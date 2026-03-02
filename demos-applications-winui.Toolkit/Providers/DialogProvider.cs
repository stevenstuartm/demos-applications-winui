using System;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using demos_applications_winui.Toolkit.Platform;

namespace demos_applications_winui.Toolkit.Providers;

/// <summary>
/// Shows WinUI <see cref="ContentDialog"/> confirmations. Requires
/// <see cref="SetXamlRoot"/> to be called after the root frame loads —
/// ContentDialog cannot render without a valid XamlRoot.
/// </summary>
public class DialogProvider : IDialogProvider
{
    private XamlRoot? _xamlRoot;

    public void SetXamlRoot(XamlRoot xamlRoot) => _xamlRoot = xamlRoot;

    public async Task<bool> ShowConfirmationAsync(string title, string message)
    {
        if (_xamlRoot is null)
            throw new InvalidOperationException(
                "XamlRoot has not been set. Call SetXamlRoot() after the root frame is loaded.");

        var dialog = new ContentDialog
        {
            Title = title,
            Content = message,
            PrimaryButtonText = "Yes",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Close,
            XamlRoot = _xamlRoot
        };

        var result = await dialog.ShowAsync();
        return result == ContentDialogResult.Primary;
    }
}
