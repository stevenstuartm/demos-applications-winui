using System.Threading.Tasks;

namespace demos_applications_winui.Toolkit.Navigation;

public interface INavigable
{
    Task OnNavigatedToAsync(NavigationContext context);
    void OnNavigatedFrom();
    Task<bool> CanNavigateFromAsync() => Task.FromResult(true);
}
