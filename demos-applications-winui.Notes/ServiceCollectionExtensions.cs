using demos_applications_winui.Notes.Services;
using demos_applications_winui.Notes.ViewModels;
using demos_applications_winui.Notes.Views;
using Microsoft.Extensions.DependencyInjection;

namespace demos_applications_winui.Notes;

/// <summary>
/// Registers Notes domain services (singleton) and pages/VMs (transient).
/// </summary>
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddNotes(this IServiceCollection services)
    {
        services.AddSingleton<INotesClient, NotesClient>();
        services.AddSingleton<INotesService, NotesService>();

        services.AddTransient<AllNotesViewModel>();
        services.AddTransient<NoteViewModel>();

        services.AddTransient<AllNotesPage>();
        services.AddTransient<NotePage>();

        return services;
    }
}
