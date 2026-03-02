using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace demos_applications_winui.Toolkit.Navigation;

/// <summary>
/// Read-only, bindable navigation state. Views bind to this (e.g., TitleBar back-button
/// visibility) without depending on <see cref="NavigationService"/> internals.
/// </summary>
public interface INavigationState : INotifyPropertyChanged
{
    bool CanGoBack { get; }
    Type? CurrentPageType { get; }
    IReadOnlyList<NavigationStackEntry>? BackStack { get; }
}
