using System;

namespace demos_applications_winui.Core.Platform;

public interface IWindowHandleProvider
{
    IntPtr WindowHandle { get; }
}
