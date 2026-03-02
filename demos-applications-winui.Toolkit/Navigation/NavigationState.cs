using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;

namespace demos_applications_winui.Toolkit.Navigation;

public partial class NavigationState : ObservableObject, INavigationState
{
    [ObservableProperty]
    public partial bool CanGoBack { get; internal set; }

    [ObservableProperty]
    public partial Type? CurrentPageType { get; internal set; }

    [ObservableProperty]
    public partial IReadOnlyList<NavigationStackEntry>? BackStack { get; internal set; }
}
