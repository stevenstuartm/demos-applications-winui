using System;

namespace demos_applications_winui.Toolkit.Navigation;

public record NavigationStackEntry(Type PageType, object? Parameter);
