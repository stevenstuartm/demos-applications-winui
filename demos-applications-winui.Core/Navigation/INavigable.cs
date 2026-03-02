using System.Threading.Tasks;

namespace demos_applications_winui.Core.Navigation;

public interface INavigable
{
    Task OnNavigatedToAsync(NavigationContext context);
    void OnNavigatedFrom();
}
