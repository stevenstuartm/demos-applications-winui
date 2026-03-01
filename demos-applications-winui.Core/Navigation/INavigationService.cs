using System.ComponentModel;
using Microsoft.UI.Xaml.Controls;

namespace demos_applications_winui.Core.Navigation;

public interface INavigationService : INotifyPropertyChanged
{
    bool CanGoBack { get; }
    void GoBack();
    void NavigateTo<TPage>(object? parameter = null) where TPage : Page;
    void NavigateAndReplace<TPage>(object? parameter = null) where TPage : Page;
    void SetFrame(Frame frame);
}
