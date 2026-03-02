using System;
using Microsoft.UI.Xaml;
using WinRT.Interop;
using demos_applications_winui.Core.Platform;

namespace demos_applications_winui.Toolkit.Providers;

public class WindowHandleProvider(Window window) : IWindowHandleProvider
{
    public IntPtr WindowHandle { get; } = WindowNative.GetWindowHandle(window);
}
