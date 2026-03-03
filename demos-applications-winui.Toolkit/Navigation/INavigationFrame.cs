namespace demos_applications_winui.Toolkit.Navigation;

/// <summary>
/// Abstracts the content surface used by <see cref="NavigationService"/> to display pages.
/// The production implementation wraps a WinUI <c>Frame</c>; tests supply a mock.
/// </summary>
public interface INavigationFrame
{
    object? Content { get; set; }
}
