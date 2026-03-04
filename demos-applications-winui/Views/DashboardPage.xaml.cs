using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;
using demos_applications_winui.Toolkit.Navigation;
using demos_applications_winui.ViewModels;

namespace demos_applications_winui.Views;

public sealed partial class DashboardPage : Page, INavigable
{
    public DashboardViewModel ViewModel { get; }

    public DashboardPage(DashboardViewModel viewModel)
    {
        ViewModel = viewModel;
        DataContext = viewModel;
        InitializeComponent();
    }

    public Task InitializeAsync(object? parameter) => ViewModel.InitializeAsync(parameter);
}
