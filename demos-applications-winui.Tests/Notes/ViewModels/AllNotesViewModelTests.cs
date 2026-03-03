using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using demos_applications_winui.Notes.Models;
using demos_applications_winui.Notes.Services;
using demos_applications_winui.Notes.ViewModels;
using demos_applications_winui.Notes.Views;
using demos_applications_winui.Toolkit.Navigation;
using Xunit;

namespace demos_applications_winui.Tests.Notes.ViewModels;

public class AllNotesViewModelTests
{
    private static (AllNotesViewModel vm, INotesService notesService, INavigationService nav) CreateVm()
    {
        var notesService = Substitute.For<INotesService>();
        var nav = Substitute.For<INavigationService>();
        var vm = new AllNotesViewModel(notesService, nav, NullLogger<AllNotesViewModel>.Instance);
        return (vm, notesService, nav);
    }

    [Fact]
    public async Task OnNavigatedToAsync_LoadsNotes()
    {
        var (vm, notesService, _) = CreateVm();
        var notes = new List<Note>
        {
            new() { Id = "1", Title = "Note 1" },
            new() { Id = "2", Title = "Note 2" }
        };
        notesService.LoadNotesAsync().Returns(notes);

        await vm.OnNavigatedToAsync(new NavigationContext(null, NavigationMode.New));

        vm.Notes.Should().HaveCount(2);
        vm.Notes[0].Title.Should().Be("Note 1");
        vm.Notes[1].Title.Should().Be("Note 2");
    }

    [Fact]
    public async Task OnNavigatedToAsync_ClearsPreviousNotes()
    {
        var (vm, notesService, _) = CreateVm();
        notesService.LoadNotesAsync().Returns(new List<Note>
        {
            new() { Id = "1", Title = "Old" }
        });
        await vm.OnNavigatedToAsync(new NavigationContext(null, NavigationMode.New));

        // Second navigation should clear and reload
        notesService.LoadNotesAsync().Returns(new List<Note>
        {
            new() { Id = "2", Title = "New" }
        });
        await vm.OnNavigatedToAsync(new NavigationContext(null, NavigationMode.Back));

        vm.Notes.Should().HaveCount(1);
        vm.Notes[0].Title.Should().Be("New");
    }

    [Fact]
    public async Task AddNoteAsync_CreatesNoteAndNavigates()
    {
        var (vm, _, nav) = CreateVm();

        await vm.AddNoteCommand.ExecuteAsync(null);

        await nav.Received(1).NavigateToAsync<NotePage>(
            Arg.Is<Note>(n =>
                n.Id != null &&
                n.Attachments != null));
    }

    [Fact]
    public async Task AddNoteAsync_CreatesNoteWithTimestamps()
    {
        var (vm, _, nav) = CreateVm();
        Note? capturedNote = null;
        await nav.NavigateToAsync<NotePage>(Arg.Do<object?>(p => capturedNote = p as Note));

        await vm.AddNoteCommand.ExecuteAsync(null);

        capturedNote.Should().NotBeNull();
        capturedNote!.CreatedDate.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(5));
        capturedNote.ModifiedDate.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task OpenNoteAsync_NavigatesWithNote()
    {
        var (vm, _, nav) = CreateVm();
        var note = new Note { Id = "123", Title = "Existing" };

        await vm.OpenNoteCommand.ExecuteAsync(note);

        await nav.Received(1).NavigateToAsync<NotePage>(note);
    }

    [Fact]
    public async Task OnNavigatedToAsync_EmptyList_ShowsNoNotes()
    {
        var (vm, notesService, _) = CreateVm();
        notesService.LoadNotesAsync().Returns(new List<Note>());

        await vm.OnNavigatedToAsync(new NavigationContext(null, NavigationMode.New));

        vm.Notes.Should().BeEmpty();
    }

    [Fact]
    public void OnNavigatedFrom_DoesNotThrow()
    {
        var (vm, _, _) = CreateVm();

        var act = () => vm.OnNavigatedFrom();

        act.Should().NotThrow();
    }
}
