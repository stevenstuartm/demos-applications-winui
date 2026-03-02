using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using demos_applications_winui.Notes.Models;

namespace demos_applications_winui.Notes.Services;

/// <summary>
/// Domain-level note operations. Adds business logic on top of <see cref="INotesClient"/> —
/// e.g., attachment type classification from file extension.
/// </summary>
public interface INotesService
{
    Task<IReadOnlyList<Note>> LoadNotesAsync();
    Task SaveNoteAsync(Note note);
    Task DeleteNoteAsync(string noteId);
    Task<NoteAttachment> AddAttachmentAsync(string noteId, string fileName, Stream content);
    Task RemoveAttachmentAsync(string noteId, string attachmentId, string relativePath);
    string GetAttachmentAbsolutePath(string noteId, string relativePath);
}
