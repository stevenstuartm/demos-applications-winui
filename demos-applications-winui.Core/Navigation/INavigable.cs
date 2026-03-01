using System.Threading.Tasks;

namespace demos_applications_winui.Core.Navigation;

public enum NavigationMode { New, Back }

public record NavigationContext(object? Parameter, NavigationMode Mode);

public interface INavigable
{
    Task OnNavigatedToAsync(NavigationContext context);
    void OnNavigatedFrom();
}
