using System;

namespace demos_applications_winui.Toolkit.Navigation;

/// <summary>
/// Snapshot of a page and its parameter stored in the manual back stack.
/// </summary>
public record NavigationStackEntry(Type PageType, object? Parameter);
