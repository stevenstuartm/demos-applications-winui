using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using demos_applications_winui.Notes.Models;
using Windows.Storage;

namespace demos_applications_winui.Notes.Services;

/// <summary>
/// File-system storage client. Each note lives in its own folder as <c>note.json</c>
/// with an optional <c>attachments/</c> subfolder. Uses <see cref="NoteJsonContext"/>
/// for AOT-safe serialization. File names are prefixed with a GUID to avoid collisions.
/// </summary>
public class NotesClient : INotesClient
{
    private static string? _notesRootPath;
    private static string NotesRootPath => _notesRootPath ??= Path.Combine(
        ApplicationData.Current.LocalFolder.Path, "notes");

    public Task<IReadOnlyList<Note>> GetAllNotesAsync()
    {
        var notes = new List<Note>();

        if (!Directory.Exists(NotesRootPath))
            return Task.FromResult<IReadOnlyList<Note>>(notes);

        foreach (var noteDir in Directory.GetDirectories(NotesRootPath))
        {
            var jsonPath = Path.Combine(noteDir, "note.json");
            if (!File.Exists(jsonPath))
                continue;

            var json = File.ReadAllText(jsonPath);
            var note = JsonSerializer.Deserialize(json, NoteJsonContext.Default.Note);
            if (note is not null)
                notes.Add(note);
        }

        return Task.FromResult<IReadOnlyList<Note>>(notes);
    }

    public Task SaveNoteAsync(Note note)
    {
        var noteDir = GetNoteDirectory(note.Id!);
        Directory.CreateDirectory(noteDir);

        var json = JsonSerializer.Serialize(note, NoteJsonContext.Default.Note);
        File.WriteAllText(Path.Combine(noteDir, "note.json"), json);

        return Task.CompletedTask;
    }

    public Task DeleteNoteAsync(string noteId)
    {
        var noteDir = GetNoteDirectory(noteId);
        if (Directory.Exists(noteDir))
            Directory.Delete(noteDir, recursive: true);

        return Task.CompletedTask;
    }

    public async Task<string> SaveAttachmentAsync(string noteId, string fileName, Stream content)
    {
        var attachmentsDir = Path.Combine(GetNoteDirectory(noteId), "attachments");
        Directory.CreateDirectory(attachmentsDir);

        var safeFileName = $"{Guid.NewGuid():N}_{fileName}";
        var filePath = Path.Combine(attachmentsDir, safeFileName);

        using var fileStream = File.Create(filePath);
        await content.CopyToAsync(fileStream);

        return safeFileName;
    }

    public Task DeleteAttachmentAsync(string noteId, string relativePath)
    {
        var filePath = Path.Combine(GetNoteDirectory(noteId), "attachments", relativePath);
        if (File.Exists(filePath))
            File.Delete(filePath);

        return Task.CompletedTask;
    }

    public string GetAttachmentAbsolutePath(string noteId, string relativePath) =>
        Path.Combine(GetNoteDirectory(noteId), "attachments", relativePath);

    private static string GetNoteDirectory(string noteId) =>
        Path.Combine(NotesRootPath, noteId);
}
