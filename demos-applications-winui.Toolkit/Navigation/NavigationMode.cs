namespace demos_applications_winui.Toolkit.Navigation;

/// <summary>
/// Indicates how a page was reached — forward navigation or back-stack traversal.
/// Passed to <see cref="INavigable.OnNavigatedToAsync"/> so pages can
/// differentiate initial load from return visits.
/// </summary>
public enum NavigationMode { New, Back }
