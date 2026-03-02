namespace demos_applications_winui.Toolkit.Navigation;

/// <summary>
/// Delivered to <see cref="INavigable.OnNavigatedToAsync"/> with the navigation
/// parameter and whether this is a forward or back navigation.
/// </summary>
public record NavigationContext(object? Parameter, NavigationMode Mode);
