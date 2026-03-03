using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using demos_applications_winui.Notes.Models;
using demos_applications_winui.Notes.Services;
using Xunit;

namespace demos_applications_winui.Tests.Notes.Services;

public class NotesServiceTests
{
    private static (NotesService service, INotesClient client) CreateService()
    {
        var client = Substitute.For<INotesClient>();
        var service = new NotesService(client);
        return (service, client);
    }

    [Fact]
    public async Task LoadNotesAsync_DelegatesToClient()
    {
        var (service, client) = CreateService();
        var expected = new List<Note> { new() { Id = "1", Title = "Test" } };
        client.GetAllNotesAsync().Returns(expected);

        var result = await service.LoadNotesAsync();

        result.Should().BeSameAs(expected);
    }

    [Fact]
    public async Task SaveNoteAsync_DelegatesToClient()
    {
        var (service, client) = CreateService();
        var note = new Note { Id = "1" };

        await service.SaveNoteAsync(note);

        await client.Received(1).SaveNoteAsync(note);
    }

    [Fact]
    public async Task DeleteNoteAsync_DelegatesToClient()
    {
        var (service, client) = CreateService();

        await service.DeleteNoteAsync("note-1");

        await client.Received(1).DeleteNoteAsync("note-1");
    }

    [Theory]
    [InlineData(".jpg", NoteAttachmentType.Image)]
    [InlineData(".jpeg", NoteAttachmentType.Image)]
    [InlineData(".png", NoteAttachmentType.Image)]
    [InlineData(".gif", NoteAttachmentType.Image)]
    [InlineData(".bmp", NoteAttachmentType.Image)]
    [InlineData(".webp", NoteAttachmentType.Image)]
    [InlineData(".svg", NoteAttachmentType.Image)]
    public async Task AddAttachmentAsync_ImageExtension_ClassifiesAsImage(string ext, NoteAttachmentType expected)
    {
        var (service, client) = CreateService();
        var fileName = $"file{ext}";
        using var stream = new MemoryStream(new byte[100]);
        client.SaveAttachmentAsync("n1", fileName, stream).Returns("saved_file");

        var attachment = await service.AddAttachmentAsync("n1", fileName, stream);

        attachment.AttachmentType.Should().Be(expected);
    }

    [Theory]
    [InlineData(".pdf", NoteAttachmentType.Document)]
    [InlineData(".docx", NoteAttachmentType.Document)]
    [InlineData(".doc", NoteAttachmentType.Document)]
    [InlineData(".xlsx", NoteAttachmentType.Document)]
    [InlineData(".xls", NoteAttachmentType.Document)]
    [InlineData(".pptx", NoteAttachmentType.Document)]
    [InlineData(".txt", NoteAttachmentType.Document)]
    [InlineData(".csv", NoteAttachmentType.Document)]
    public async Task AddAttachmentAsync_DocumentExtension_ClassifiesAsDocument(string ext, NoteAttachmentType expected)
    {
        var (service, client) = CreateService();
        var fileName = $"file{ext}";
        using var stream = new MemoryStream(new byte[100]);
        client.SaveAttachmentAsync("n1", fileName, stream).Returns("saved_file");

        var attachment = await service.AddAttachmentAsync("n1", fileName, stream);

        attachment.AttachmentType.Should().Be(expected);
    }

    [Theory]
    [InlineData(".zip")]
    [InlineData(".exe")]
    [InlineData(".unknown")]
    [InlineData("")]
    public async Task AddAttachmentAsync_UnknownExtension_ClassifiesAsOther(string ext)
    {
        var (service, client) = CreateService();
        var fileName = ext == "" ? "noextension" : $"file{ext}";
        using var stream = new MemoryStream(new byte[100]);
        client.SaveAttachmentAsync("n1", fileName, stream).Returns("saved_file");

        var attachment = await service.AddAttachmentAsync("n1", fileName, stream);

        attachment.AttachmentType.Should().Be(NoteAttachmentType.Other);
    }

    [Fact]
    public async Task AddAttachmentAsync_CaseInsensitive_ClassifiesCorrectly()
    {
        var (service, client) = CreateService();
        using var stream = new MemoryStream(new byte[100]);
        client.SaveAttachmentAsync("n1", "photo.JPG", stream).Returns("saved");

        var attachment = await service.AddAttachmentAsync("n1", "photo.JPG", stream);

        attachment.AttachmentType.Should().Be(NoteAttachmentType.Image);
    }

    [Fact]
    public async Task AddAttachmentAsync_SetsFileNameAndRelativePath()
    {
        var (service, client) = CreateService();
        using var stream = new MemoryStream(new byte[50]);
        client.SaveAttachmentAsync("n1", "doc.pdf", stream).Returns("abc_doc.pdf");

        var attachment = await service.AddAttachmentAsync("n1", "doc.pdf", stream);

        attachment.FileName.Should().Be("doc.pdf");
        attachment.RelativePath.Should().Be("abc_doc.pdf");
        attachment.FileSizeBytes.Should().Be(50);
    }

    [Fact]
    public async Task AddAttachmentAsync_GeneratesUniqueId()
    {
        var (service, client) = CreateService();
        using var stream = new MemoryStream(new byte[10]);
        client.SaveAttachmentAsync("n1", "a.txt", stream).Returns("saved");

        var attachment = await service.AddAttachmentAsync("n1", "a.txt", stream);

        attachment.Id.Should().NotBeNullOrEmpty();
        attachment.Id.Should().HaveLength(32); // Guid.ToString("N") = 32 hex chars
    }

    [Fact]
    public async Task RemoveAttachmentAsync_DelegatesToClient()
    {
        var (service, client) = CreateService();

        await service.RemoveAttachmentAsync("n1", "att1", "path/file.jpg");

        await client.Received(1).DeleteAttachmentAsync("n1", "path/file.jpg");
    }

    [Fact]
    public void GetAttachmentAbsolutePath_DelegatesToClient()
    {
        var (service, client) = CreateService();
        client.GetAttachmentAbsolutePath("n1", "file.jpg").Returns("/full/path/file.jpg");

        var result = service.GetAttachmentAbsolutePath("n1", "file.jpg");

        result.Should().Be("/full/path/file.jpg");
    }
}
