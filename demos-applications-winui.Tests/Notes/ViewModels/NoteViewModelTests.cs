using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using demos_applications_winui.Notes.Models;
using demos_applications_winui.Notes.Services;
using demos_applications_winui.Notes.ViewModels;
using demos_applications_winui.Toolkit.Navigation;
using demos_applications_winui.Toolkit.Platform;
using Xunit;

namespace demos_applications_winui.Tests.Notes.ViewModels;

public class NoteViewModelTests
{
    private static NoteViewModelContext CreateVm()
    {
        var notesService = Substitute.For<INotesService>();
        var navigationService = Substitute.For<INavigationService>();
        var dialogProvider = Substitute.For<IDialogProvider>();
        var filePickerProvider = Substitute.For<IFilePickerProvider>();
        var cameraProvider = Substitute.For<ICameraProvider>();
        var toastProvider = Substitute.For<IToastProvider>();
        var editorBridge = Substitute.For<IHtmlEditorBridge>();

        var vm = new NoteViewModel(
            notesService,
            navigationService,
            dialogProvider,
            filePickerProvider,
            cameraProvider,
            toastProvider,
            NullLogger<NoteViewModel>.Instance);

        vm.SetEditorBridge(editorBridge);

        return new NoteViewModelContext(
            vm, notesService, navigationService, dialogProvider,
            filePickerProvider, cameraProvider, toastProvider, editorBridge);
    }

    private static Note CreateTestNote(string id = "test-id", string title = "Test Note") => new()
    {
        Id = id,
        Title = title,
        HtmlContent = "<p>Hello</p>",
        CreatedDate = new DateTime(2025, 1, 1),
        ModifiedDate = new DateTime(2025, 1, 2),
        Attachments = []
    };

    [Fact]
    public async Task OnNavigatedToAsync_LoadsNoteData()
    {
        var ctx = CreateVm();
        var note = CreateTestNote();

        await ctx.Vm.OnNavigatedToAsync(new NavigationContext(note, NavigationMode.New));

        ctx.Vm.Title.Should().Be("Test Note");
        ctx.Vm.CreatedDate.Should().Be(new DateTime(2025, 1, 1));
        ctx.Vm.IsEditMode.Should().BeTrue();
    }

    [Fact]
    public async Task OnNavigatedToAsync_SetsEditorContent()
    {
        var ctx = CreateVm();
        var note = CreateTestNote();
        ctx.Vm.IsEditorReady = true;

        await ctx.Vm.OnNavigatedToAsync(new NavigationContext(note, NavigationMode.New));

        await ctx.EditorBridge.Received(1).SetContentAsync("<p>Hello</p>");
    }

    [Fact]
    public async Task OnNavigatedToAsync_SuppressesDirtyTracking()
    {
        var ctx = CreateVm();
        var note = CreateTestNote();

        await ctx.Vm.OnNavigatedToAsync(new NavigationContext(note, NavigationMode.New));

        ctx.Vm.IsDirty.Should().BeFalse();
    }

    [Fact]
    public async Task OnNavigatedToAsync_LoadsAttachments()
    {
        var ctx = CreateVm();
        var note = CreateTestNote();
        note.Attachments = [
            new NoteAttachment { Id = "a1", FileName = "photo.jpg" },
            new NoteAttachment { Id = "a2", FileName = "doc.pdf" }
        ];

        await ctx.Vm.OnNavigatedToAsync(new NavigationContext(note, NavigationMode.New));

        ctx.Vm.Attachments.Should().HaveCount(2);
        ctx.Vm.HasAttachments.Should().BeTrue();
    }

    [Fact]
    public void TitleChanged_SetsDirtyFlag()
    {
        var ctx = CreateVm();
        _ = ctx.Vm.OnNavigatedToAsync(new NavigationContext(CreateTestNote(), NavigationMode.New));

        ctx.Vm.Title = "Changed Title";

        ctx.Vm.IsDirty.Should().BeTrue();
    }

    [Fact]
    public void TitleChanged_ValidatesRequired()
    {
        var ctx = CreateVm();
        _ = ctx.Vm.OnNavigatedToAsync(new NavigationContext(CreateTestNote(), NavigationMode.New));

        ctx.Vm.Title = null;

        ctx.Vm.ShowTitleError.Should().BeTrue();
        ctx.Vm.TitleErrorMessage.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void TitleChanged_ValidTitle_ClearsError()
    {
        var ctx = CreateVm();
        _ = ctx.Vm.OnNavigatedToAsync(new NavigationContext(CreateTestNote(), NavigationMode.New));
        ctx.Vm.Title = null; // trigger error
        ctx.Vm.Title = "Valid Title";

        ctx.Vm.ShowTitleError.Should().BeFalse();
    }

    [Fact]
    public void PageTitle_WhenDirty_ShowsUnsavedChanges()
    {
        var ctx = CreateVm();

        ctx.Vm.IsDirty = true;

        ctx.Vm.PageTitle.Should().Be("* Unsaved changes");
    }

    [Fact]
    public void PageTitle_WhenClean_ShowsNote()
    {
        var ctx = CreateVm();

        ctx.Vm.IsDirty = false;

        ctx.Vm.PageTitle.Should().Be("Note");
    }

    [Fact]
    public async Task CanNavigateFromAsync_WhenNotDirty_ReturnsTrue()
    {
        var ctx = CreateVm();
        ctx.Vm.IsDirty = false;

        var result = await ctx.Vm.CanNavigateFromAsync();

        result.Should().BeTrue();
    }

    [Fact]
    public async Task CanNavigateFromAsync_WhenDirty_ShowsConfirmation()
    {
        var ctx = CreateVm();
        ctx.Vm.IsDirty = true;
        ctx.DialogProvider.ShowConfirmationAsync(Arg.Any<string>(), Arg.Any<string>())
            .Returns(true);

        await ctx.Vm.CanNavigateFromAsync();

        await ctx.DialogProvider.Received(1).ShowConfirmationAsync(
            Arg.Any<string>(), Arg.Any<string>());
    }

    [Fact]
    public async Task CanNavigateFromAsync_WhenDirty_UserConfirms_ReturnsTrue()
    {
        var ctx = CreateVm();
        ctx.Vm.IsDirty = true;
        ctx.DialogProvider.ShowConfirmationAsync(Arg.Any<string>(), Arg.Any<string>())
            .Returns(true);

        var result = await ctx.Vm.CanNavigateFromAsync();

        result.Should().BeTrue();
        ctx.Vm.IsDirty.Should().BeFalse();
    }

    [Fact]
    public async Task CanNavigateFromAsync_WhenDirty_UserCancels_ReturnsFalse()
    {
        var ctx = CreateVm();
        ctx.Vm.IsDirty = true;
        ctx.DialogProvider.ShowConfirmationAsync(Arg.Any<string>(), Arg.Any<string>())
            .Returns(false);

        var result = await ctx.Vm.CanNavigateFromAsync();

        result.Should().BeFalse();
    }

    [Fact]
    public async Task SaveAsync_WithValidTitle_SavesAndClearsDirty()
    {
        var ctx = CreateVm();
        var note = CreateTestNote();
        await ctx.Vm.OnNavigatedToAsync(new NavigationContext(note, NavigationMode.New));
        ctx.Vm.Title = "Updated Title";
        ctx.EditorBridge.GetContentAsync().Returns("<p>Updated</p>");

        await ctx.Vm.SaveCommand.ExecuteAsync(null);

        await ctx.NotesService.Received(1).SaveNoteAsync(note);
        ctx.Vm.IsDirty.Should().BeFalse();
    }

    [Fact]
    public async Task SaveAsync_WithValidTitle_ShowsSuccessToast()
    {
        var ctx = CreateVm();
        var note = CreateTestNote();
        await ctx.Vm.OnNavigatedToAsync(new NavigationContext(note, NavigationMode.New));
        ctx.Vm.Title = "Valid";
        ctx.EditorBridge.GetContentAsync().Returns("<p>content</p>");

        await ctx.Vm.SaveCommand.ExecuteAsync(null);

        ctx.ToastProvider.Received(1).ShowSuccess(Arg.Any<string>(), Arg.Any<string>());
    }

    [Fact]
    public async Task SaveAsync_WithEmptyTitle_ShowsValidationError()
    {
        var ctx = CreateVm();
        var note = CreateTestNote();
        await ctx.Vm.OnNavigatedToAsync(new NavigationContext(note, NavigationMode.New));
        ctx.Vm.Title = null;

        await ctx.Vm.SaveCommand.ExecuteAsync(null);

        ctx.ToastProvider.Received(1).ShowError(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<bool>());
        await ctx.NotesService.DidNotReceive().SaveNoteAsync(Arg.Any<Note>());
    }

    [Fact]
    public async Task DeleteAsync_UserConfirms_DeletesAndNavigatesBack()
    {
        var ctx = CreateVm();
        var note = CreateTestNote();
        await ctx.Vm.OnNavigatedToAsync(new NavigationContext(note, NavigationMode.New));
        ctx.DialogProvider.ShowConfirmationAsync(Arg.Any<string>(), Arg.Any<string>())
            .Returns(true);

        await ctx.Vm.DeleteCommand.ExecuteAsync(null);

        await ctx.NotesService.Received(1).DeleteNoteAsync("test-id");
        await ctx.NavigationService.Received(1).GoBackAsync();
    }

    [Fact]
    public async Task DeleteAsync_UserCancels_DoesNotDelete()
    {
        var ctx = CreateVm();
        var note = CreateTestNote();
        await ctx.Vm.OnNavigatedToAsync(new NavigationContext(note, NavigationMode.New));
        ctx.DialogProvider.ShowConfirmationAsync(Arg.Any<string>(), Arg.Any<string>())
            .Returns(false);

        await ctx.Vm.DeleteCommand.ExecuteAsync(null);

        await ctx.NotesService.DidNotReceive().DeleteNoteAsync(Arg.Any<string>());
    }

    [Fact]
    public async Task DiscardChangesAsync_RestoresOriginalContent()
    {
        var ctx = CreateVm();
        var note = CreateTestNote();
        await ctx.Vm.OnNavigatedToAsync(new NavigationContext(note, NavigationMode.New));
        ctx.Vm.Title = "Changed";
        ctx.Vm.IsDirty = true;

        await ctx.Vm.DiscardChangesCommand.ExecuteAsync(null);

        ctx.Vm.Title.Should().Be("Test Note");
        ctx.Vm.IsDirty.Should().BeFalse();
        await ctx.EditorBridge.Received().SetContentAsync("<p>Hello</p>");
    }

    [Fact]
    public void EditorContentChanged_SetsDirtyFlag()
    {
        var ctx = CreateVm();
        _ = ctx.Vm.OnNavigatedToAsync(new NavigationContext(CreateTestNote(), NavigationMode.New));

        ctx.EditorBridge.ContentChanged += Raise.Event<EventHandler<string>>(this, "<p>new</p>");

        ctx.Vm.IsDirty.Should().BeTrue();
    }

    [Fact]
    public void OnNavigatedFrom_CancelsPendingCommands()
    {
        var ctx = CreateVm();

        var act = () => ctx.Vm.OnNavigatedFrom();

        act.Should().NotThrow();
    }

    [Fact]
    public void IsEditorLoading_InverseOfIsEditorReady()
    {
        var ctx = CreateVm();

        ctx.Vm.IsEditorReady = false;
        ctx.Vm.IsEditorLoading.Should().BeTrue();

        ctx.Vm.IsEditorReady = true;
        ctx.Vm.IsEditorLoading.Should().BeFalse();
    }

    private record NoteViewModelContext(
        NoteViewModel Vm,
        INotesService NotesService,
        INavigationService NavigationService,
        IDialogProvider DialogProvider,
        IFilePickerProvider FilePickerProvider,
        ICameraProvider CameraProvider,
        IToastProvider ToastProvider,
        IHtmlEditorBridge EditorBridge);
}
