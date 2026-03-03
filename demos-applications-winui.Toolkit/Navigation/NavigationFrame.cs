using Microsoft.UI.Xaml.Controls;

namespace demos_applications_winui.Toolkit.Navigation;

/// <summary>
/// Production implementation of <see cref="INavigationFrame"/> that delegates
/// to a WinUI <see cref="Frame"/>. Created once in MainWindow and handed to
/// <see cref="NavigationService"/> via <see cref="INavigationService.SetFrame"/>.
/// </summary>
public class NavigationFrame(Frame frame) : INavigationFrame
{
    public object? Content
    {
        get => frame.Content;
        set => frame.Content = value;
    }
}
