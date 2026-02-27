using Microsoft.UI.Xaml.Controls;

namespace demos_applications_winui.Services;

public interface INavigationService
{
    bool CanGoBack { get; }
    void GoBack();
    void NavigateTo<TPage>(object? parameter = null) where TPage : Page;
    void SetFrame(Frame frame);
}
