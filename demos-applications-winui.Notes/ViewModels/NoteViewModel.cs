using System;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using demos_applications_winui.Toolkit.Navigation;
using demos_applications_winui.Toolkit.Platform;
using demos_applications_winui.Notes.Models;
using demos_applications_winui.Notes.Services;

namespace demos_applications_winui.Notes.ViewModels;

/// <summary>
/// Detail/edit VM for a single note. Transient — created fresh per navigation.
/// Uses <see cref="ObservableValidator"/> for title validation, dirty-tracking via
/// <see cref="_suppressDirtyTracking"/> to distinguish user edits from programmatic loads,
/// and <see cref="INavigable.CanNavigateFromAsync"/> to prompt before discarding unsaved changes.
/// The <see cref="IHtmlEditorBridge"/> is set by the view after WebView2 initializes.
/// </summary>
public partial class NoteViewModel(
    INotesService notesService,
    INavigationService navigationService,
    IDialogProvider dialogProvider,
    IFilePickerProvider filePickerProvider,
    ICameraProvider cameraProvider,
    IToastProvider toastProvider,
    ILogger<NoteViewModel> logger) : ObservableValidator, INavigable, IDisposable
{
    private Note? _currentNote;
    private IHtmlEditorBridge? _editorBridge;
    private string? _savedHtmlContent;
    private bool _suppressDirtyTracking;

    [ObservableProperty]
    [Required(ErrorMessage = "Title is required")]
    [MinLength(1, ErrorMessage = "Title cannot be empty")]
    public partial string? Title { get; set; }

    [ObservableProperty]
    public partial DateTime CreatedDate { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsEditorLoading))]
    public partial bool IsEditorReady { get; set; }

    public bool IsEditorLoading => !IsEditorReady;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsReadOnly))]
    public partial bool IsEditMode { get; set; }

    public bool IsReadOnly => !IsEditMode;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PageTitle))]
    public partial bool IsDirty { get; set; }

    [ObservableProperty]
    public partial string? TitleErrorMessage { get; set; }

    [ObservableProperty]
    public partial bool ShowTitleError { get; set; }

    public string PageTitle => IsDirty ? "* Unsaved changes" : "Note";

    [ObservableProperty]
    public partial bool HasAttachments { get; set; }

    public ObservableCollection<AttachmentItemViewModel> Attachments { get; } = [];

    /// <summary>
    /// Called by the view after WebView2 initializes. If <see cref="InitializeAsync"/>
    /// already ran (which it will — the bridge isn't ready during init), this loads
    /// the stored content into the editor.
    /// </summary>
    public async void SetEditorBridge(IHtmlEditorBridge bridge)
    {
        if (_editorBridge is not null)
            _editorBridge.ContentChanged -= OnEditorContentChanged;

        _editorBridge = bridge;
        _editorBridge.ContentChanged += OnEditorContentChanged;

        if (_savedHtmlContent is not null)
            await _editorBridge.SetContentAsync(_savedHtmlContent);

        await _editorBridge.SetReadOnlyAsync(!IsEditMode);
    }

    public async Task InitializeAsync(object? parameter)
    {
        if (parameter is Note note)
        {
            LogLoadingNote(note.Id);
            _suppressDirtyTracking = true;

            _currentNote = note;
            Title = note.Title;
            CreatedDate = note.CreatedDate;
            _savedHtmlContent = note.HtmlContent;
            IsEditMode = false;

            Attachments.Clear();
            if (note.Attachments is not null)
            {
                foreach (var attachment in note.Attachments)
                    Attachments.Add(CreateAttachmentItem(attachment));
            }
            HasAttachments = Attachments.Count > 0;

            if (_editorBridge is not null && IsEditorReady)
                await _editorBridge.SetContentAsync(note.HtmlContent ?? string.Empty);

            IsDirty = false;
            ShowTitleError = false;
            ClearErrors();

            _suppressDirtyTracking = false;
        }
    }

    public async Task<bool> CanNavigateFromAsync()
    {
        if (!IsDirty) return true;

        var confirmed = await dialogProvider.ShowConfirmationAsync(
            UserMessages.Dialog.UnsavedChangesTitle,
            UserMessages.Dialog.UnsavedChangesMessage);

        if (confirmed)
            IsDirty = false;

        return confirmed;
    }

    public void Dispose()
    {
        SaveCommand.Cancel();
        DeleteCommand.Cancel();

        if (_editorBridge is not null)
            _editorBridge.ContentChanged -= OnEditorContentChanged;
    }

    partial void OnTitleChanged(string? value)
    {
        if (!_suppressDirtyTracking)
            IsDirty = true;

        ValidateProperty(value, nameof(Title));
        var firstError = GetErrors(nameof(Title)).FirstOrDefault();
        TitleErrorMessage = firstError?.ErrorMessage;
        ShowTitleError = firstError is not null;
    }

    [RelayCommand]
    private async Task ToggleModeAsync()
    {
        if (_editorBridge is not null)
            await _editorBridge.SetReadOnlyAsync(!IsEditMode);
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (_currentNote is null || _editorBridge is null) return;

        ValidateAllProperties();
        if (HasErrors)
        {
            var firstError = GetErrors(nameof(Title)).FirstOrDefault();
            TitleErrorMessage = firstError?.ErrorMessage;
            ShowTitleError = firstError is not null;
            toastProvider.ShowError(UserMessages.Toast.ValidationErrorTitle, UserMessages.Toast.ValidationErrorMessage);
            return;
        }

        _currentNote.Title = Title;
        _currentNote.HtmlContent = await _editorBridge.GetContentAsync();
        _currentNote.ModifiedDate = DateTime.Now;
        _savedHtmlContent = _currentNote.HtmlContent;

        await notesService.SaveNoteAsync(_currentNote);

        IsDirty = false;
        ShowTitleError = false;
        toastProvider.ShowSuccess(UserMessages.Toast.SaveSuccessTitle, UserMessages.Toast.SaveSuccessMessage);
    }

    [RelayCommand]
    private async Task DeleteAsync()
    {
        if (_currentNote is null) return;

        var confirmed = await dialogProvider.ShowConfirmationAsync(
            UserMessages.Dialog.DeleteNoteTitle,
            UserMessages.Dialog.DeleteNoteMessage);

        if (!confirmed) return;

        await notesService.DeleteNoteAsync(_currentNote.Id!);
        IsDirty = false;
        await navigationService.GoBackAsync();
    }

    [RelayCommand]
    private async Task DiscardChangesAsync()
    {
        if (_editorBridge is not null)
            await _editorBridge.SetContentAsync(_savedHtmlContent ?? string.Empty);

        if (_currentNote is not null)
            Title = _currentNote.Title;

        IsDirty = false;
        ShowTitleError = false;
    }

    [RelayCommand]
    private async Task AttachFilesAsync()
    {
        if (_currentNote is null) return;

        var files = await filePickerProvider.PickFilesAsync(
            [".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp", ".pdf", ".docx", ".txt"]);

        foreach (var file in files)
        {
            using var stream = System.IO.File.OpenRead(file.Path);
            var attachment = await notesService.AddAttachmentAsync(
                _currentNote.Id!, file.FileName, stream);
            _currentNote.Attachments ??= [];
            _currentNote.Attachments.Add(attachment);
            Attachments.Add(CreateAttachmentItem(attachment));

            if (attachment.AttachmentType == NoteAttachmentType.Image && _editorBridge is not null)
            {
                var absolutePath = notesService.GetAttachmentAbsolutePath(
                    _currentNote.Id!, attachment.RelativePath!);
                var fileUri = new Uri(absolutePath).AbsoluteUri;
                await _editorBridge.InsertImageAsync(fileUri);
            }
        }

        if (files.Count > 0)
        {
            HasAttachments = Attachments.Count > 0;
            IsDirty = true;
        }
    }

    private async Task OpenAttachmentAsync(NoteAttachment attachment)
    {
        if (_currentNote is null) return;

        var absolutePath = notesService.GetAttachmentAbsolutePath(
            _currentNote.Id!, attachment.RelativePath!);
        var file = await Windows.Storage.StorageFile.GetFileFromPathAsync(absolutePath);
        await Windows.System.Launcher.LaunchFileAsync(file);
    }

    private async Task RemoveAttachmentAsync(NoteAttachment attachment)
    {
        if (_currentNote is null) return;

        await notesService.RemoveAttachmentAsync(
            _currentNote.Id!, attachment.Id!, attachment.RelativePath!);
        _currentNote.Attachments?.Remove(attachment);

        var item = Attachments.FirstOrDefault(a => a.Attachment == attachment);
        if (item is not null)
            Attachments.Remove(item);

        HasAttachments = Attachments.Count > 0;
        IsDirty = true;
    }

    [RelayCommand]
    private async Task CapturePhotoAsync()
    {
        if (_currentNote is null) return;

        var photo = await cameraProvider.CapturePhotoAsync();
        if (photo is null) return;

        var fileName = $"photo_{DateTime.Now:yyyyMMdd_HHmmss}.jpg";
        using var stream = System.IO.File.OpenRead(photo.TempFilePath);
        var attachment = await notesService.AddAttachmentAsync(
            _currentNote.Id!, fileName, stream);
        _currentNote.Attachments ??= [];
        _currentNote.Attachments.Add(attachment);
        Attachments.Add(CreateAttachmentItem(attachment));
        HasAttachments = true;

        if (_editorBridge is not null)
        {
            var absolutePath = notesService.GetAttachmentAbsolutePath(
                _currentNote.Id!, attachment.RelativePath!);
            var fileUri = new Uri(absolutePath).AbsoluteUri;
            await _editorBridge.InsertImageAsync(fileUri);
        }

        IsDirty = true;
    }

    private AttachmentItemViewModel CreateAttachmentItem(NoteAttachment attachment)
    {
        var absolutePath = notesService.GetAttachmentAbsolutePath(
            _currentNote!.Id!, attachment.RelativePath!);
        var thumbnailUri = attachment.AttachmentType == NoteAttachmentType.Image
            ? new Uri(absolutePath).AbsoluteUri
            : null;
        return new AttachmentItemViewModel(
            attachment, thumbnailUri, OpenAttachmentAsync, RemoveAttachmentAsync);
    }

    private void OnEditorContentChanged(object? sender, string html)
    {
        if (!_suppressDirtyTracking)
            IsDirty = true;
    }

    [LoggerMessage(EventId = 5000, Level = LogLevel.Debug, Message = "Loading note {NoteId}")]
    partial void LogLoadingNote(string? noteId);
}
