using System;

namespace demos_applications_winui.Toolkit.Platform;

/// <summary>
/// Provides the native HWND required by WinRT pickers and capture UIs
/// that need <c>InitializeWithWindow.Initialize</c>.
/// </summary>
public interface IWindowHandleProvider
{
    IntPtr WindowHandle { get; }
}
