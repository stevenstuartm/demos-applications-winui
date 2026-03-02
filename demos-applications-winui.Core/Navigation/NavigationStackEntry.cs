using System;

namespace demos_applications_winui.Core.Navigation;

public record NavigationStackEntry(Type PageType, object? Parameter);
