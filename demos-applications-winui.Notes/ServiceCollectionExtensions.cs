using demos_applications_winui.Notes.Services;
using demos_applications_winui.Notes.ViewModels;
using demos_applications_winui.Notes.Views;
using Microsoft.Extensions.DependencyInjection;

namespace demos_applications_winui.Notes;

/// <summary>
/// Registers all Notes domain services, view models, and pages as singletons.
/// </summary>
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddNotes(this IServiceCollection services)
    {
        services.AddSingleton<INotesClient, NotesClient>();
        services.AddSingleton<INotesService, NotesService>();

        services.AddSingleton<AllNotesViewModel>();
        services.AddSingleton<NoteViewModel>();

        services.AddSingleton<AllNotesPage>();
        services.AddSingleton<NotePage>();

        return services;
    }
}
