using System.Threading.Tasks;
using Microsoft.UI.Xaml;

namespace demos_applications_winui.Toolkit.Platform;

/// <summary>
/// Abstraction over WinUI <see cref="Microsoft.UI.Xaml.Controls.ContentDialog"/>.
/// <see cref="SetXamlRoot"/> must be called once after the root frame loads
/// before any dialogs can be shown.
/// </summary>
public interface IDialogProvider
{
    void SetXamlRoot(XamlRoot xamlRoot);
    Task<bool> ShowConfirmationAsync(string title, string message);
}
