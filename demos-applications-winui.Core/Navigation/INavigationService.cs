using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;

namespace demos_applications_winui.Core.Navigation;

public interface INavigationService
{
    Task GoBackAsync();
    Task NavigateToAsync<TPage>(object? parameter = null) where TPage : Page;
    Task NavigateAndReplaceAsync<TPage>(object? parameter = null) where TPage : Page;
    Task NavigateToDefaultAsync();
    void SetFrame(Frame frame);
}
