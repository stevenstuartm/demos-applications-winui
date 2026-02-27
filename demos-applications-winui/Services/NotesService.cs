using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using demos_applications_winui.Models;
using Windows.Storage;

namespace demos_applications_winui.Services;

public class NotesService : INotesService
{
    private readonly StorageFolder _storageFolder = ApplicationData.Current.LocalFolder;

    public async Task<IReadOnlyList<Note>> LoadNotesAsync()
    {
        var notes = new List<Note>();
        await LoadFilesRecursivelyAsync(_storageFolder, notes);
        return notes;
    }

    public async Task SaveNoteAsync(Note note)
    {
        var noteFile = await _storageFolder.TryGetItemAsync(note.Filename) as StorageFile;

        noteFile ??= await _storageFolder.CreateFileAsync(note.Filename, CreationCollisionOption.ReplaceExisting);

        await FileIO.WriteTextAsync(noteFile, note.Text);
    }

    public async Task DeleteNoteAsync(Note note)
    {
        var noteFile = await _storageFolder.TryGetItemAsync(note.Filename) as StorageFile;

        if (noteFile is not null)
        {
            await noteFile.DeleteAsync();
        }
    }

    private async Task LoadFilesRecursivelyAsync(StorageFolder folder, List<Note> notes)
    {
        var items = await folder.GetItemsAsync();

        foreach (var item in items)
        {
            if (item is StorageFolder subFolder)
            {
                await LoadFilesRecursivelyAsync(subFolder, notes);
            }
            else if (item is StorageFile file)
            {
                notes.Add(new Note
                {
                    Filename = file.Name,
                    Text = await FileIO.ReadTextAsync(file),
                    Date = file.DateCreated.DateTime
                });
            }
        }
    }
}
