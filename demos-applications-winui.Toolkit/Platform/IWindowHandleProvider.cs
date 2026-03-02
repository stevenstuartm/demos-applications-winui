using System;

namespace demos_applications_winui.Toolkit.Platform;

public interface IWindowHandleProvider
{
    IntPtr WindowHandle { get; }
}
