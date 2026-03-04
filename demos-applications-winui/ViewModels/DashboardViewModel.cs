using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using demos_applications_winui.Models;
using demos_applications_winui.Notes.Views;
using demos_applications_winui.Toolkit.Navigation;

namespace demos_applications_winui.ViewModels;

public partial class DashboardViewModel(
    INavigationService navigationService) : ObservableObject, INavigable
{
    public ObservableCollection<DashboardItem> Items { get; } = [];

    public Task InitializeAsync(object? parameter)
    {
        if (Items.Count == 0)
        {
            Items.Add(new DashboardItem
            {
                Title = "Quick Notes",
                Description = "Create and manage notes with a rich text editor",
                Glyph = "\uE70B",
                PageType = typeof(AllNotesPage)
            });
        }

        return Task.CompletedTask;
    }

    [RelayCommand]
    private async Task NavigateToItemAsync(DashboardItem item)
    {
        await navigationService.NavigateToAsync(item.PageType);
    }
}
