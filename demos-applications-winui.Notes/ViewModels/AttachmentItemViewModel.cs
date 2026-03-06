using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using demos_applications_winui.Notes.Models;

namespace demos_applications_winui.Notes.ViewModels;

/// <summary>
/// Presentation wrapper for <see cref="NoteAttachment"/>. Provides resolved
/// thumbnail URI and per-item commands so the DataTemplate can bind via x:Bind
/// without code-behind.
/// </summary>
public class AttachmentItemViewModel
{
    public AttachmentItemViewModel(
        NoteAttachment attachment,
        string? thumbnailUri,
        Func<NoteAttachment, Task> onOpen,
        Func<NoteAttachment, Task> onRemove)
    {
        Attachment = attachment;
        FileName = attachment.FileName;
        IsImage = attachment.AttachmentType == NoteAttachmentType.Image;
        ThumbnailUri = thumbnailUri;
        OpenCommand = new AsyncRelayCommand(() => onOpen(attachment));
        RemoveCommand = new AsyncRelayCommand(() => onRemove(attachment));
    }

    public NoteAttachment Attachment { get; }
    public string? FileName { get; }
    public bool IsImage { get; }
    public bool IsDocument => !IsImage;
    public string? ThumbnailUri { get; }
    public IAsyncRelayCommand OpenCommand { get; }
    public IAsyncRelayCommand RemoveCommand { get; }
}
