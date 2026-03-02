using System.Threading.Tasks;

namespace demos_applications_winui.Toolkit.Platform;

/// <summary>
/// Wraps <see cref="Windows.Media.Capture.CameraCaptureUI"/> with HWND initialization.
/// Returns null when the user cancels or capture fails.
/// </summary>
public interface ICameraProvider
{
    Task<CapturedPhoto?> CapturePhotoAsync();
}
