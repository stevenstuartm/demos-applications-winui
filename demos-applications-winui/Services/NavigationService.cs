using Microsoft.UI.Xaml.Controls;

namespace demos_applications_winui.Services;

public class NavigationService : INavigationService
{
    private Frame? _frame;

    public bool CanGoBack => _frame?.CanGoBack ?? false;

    public void SetFrame(Frame frame) => _frame = frame;

    public void GoBack()
    {
        if (_frame?.CanGoBack == true)
        {
            _frame.GoBack();
        }
    }

    public void NavigateTo<TPage>(object? parameter = null) where TPage : Page
    {
        _frame?.Navigate(typeof(TPage), parameter);
    }
}
