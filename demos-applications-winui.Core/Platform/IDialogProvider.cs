using System.Threading.Tasks;
using Microsoft.UI.Xaml;

namespace demos_applications_winui.Core.Platform;

public interface IDialogProvider
{
    void SetXamlRoot(XamlRoot xamlRoot);
    Task<bool> ShowConfirmationAsync(string title, string message);
}
