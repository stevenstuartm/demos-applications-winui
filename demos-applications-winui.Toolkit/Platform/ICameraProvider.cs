using System.Threading.Tasks;
using Microsoft.UI.Xaml;

namespace demos_applications_winui.Toolkit.Platform;

/// <summary>
/// Captures a photo using the device camera via <c>MediaCapture</c> and a live
/// preview dialog. Requires <see cref="SetXamlRoot"/> before first use.
/// Returns null when the user cancels; throws on hardware or permission errors.
/// </summary>
public interface ICameraProvider
{
    void SetXamlRoot(XamlRoot xamlRoot);
    Task<CapturedPhoto?> CapturePhotoAsync();
}
