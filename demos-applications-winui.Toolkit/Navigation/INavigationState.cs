using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace demos_applications_winui.Toolkit.Navigation;

public interface INavigationState : INotifyPropertyChanged
{
    bool CanGoBack { get; }
    Type? CurrentPageType { get; }
    IReadOnlyList<NavigationStackEntry>? BackStack { get; }
}
