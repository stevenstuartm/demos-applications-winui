using System;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using demos_applications_winui.Toolkit.Platform;
using demos_applications_winui.Toolkit.Toast;
using Xunit;

namespace demos_applications_winui.Tests.Toolkit.Toast;

public class ToastProviderTests
{
    private static (ToastProvider provider, ToastState state) CreateProvider()
    {
        var state = new ToastState();
        var provider = new ToastProvider(state, NullLogger<ToastProvider>.Instance);
        return (provider, state);
    }

    [Fact]
    public void Show_AddsToastToState()
    {
        var (provider, state) = CreateProvider();

        provider.Show("Title", "Message");

        state.Toasts.Should().HaveCount(1);
        state.Toasts[0].Title.Should().Be("Title");
        state.Toasts[0].Message.Should().Be("Message");
    }

    [Fact]
    public void Show_ReturnsToastId()
    {
        var (provider, state) = CreateProvider();

        var id = provider.Show("Title", "Message");

        id.Should().NotBe(Guid.Empty);
        state.Toasts[0].Id.Should().Be(id);
    }

    [Fact]
    public void Show_DefaultSeverity_IsInformational()
    {
        var (provider, state) = CreateProvider();

        provider.Show("Title", "Message");

        state.Toasts[0].Severity.Should().Be(ToastSeverity.Informational);
    }

    [Fact]
    public void Show_WithSeverity_SetsCorrectly()
    {
        var (provider, state) = CreateProvider();

        provider.Show("Title", "Message", ToastSeverity.Warning);

        state.Toasts[0].Severity.Should().Be(ToastSeverity.Warning);
    }

    [Fact]
    public void ShowSuccess_SetsSeverityToSuccess()
    {
        var (provider, state) = CreateProvider();

        provider.ShowSuccess("Done", "It worked");

        state.Toasts[0].Severity.Should().Be(ToastSeverity.Success);
    }

    [Fact]
    public void ShowError_SetsSeverityToError()
    {
        var (provider, state) = CreateProvider();

        provider.ShowError("Fail", "Something broke");

        state.Toasts[0].Severity.Should().Be(ToastSeverity.Error);
    }

    [Fact]
    public void Show_Persistent_MarksAsPersistent()
    {
        var (provider, state) = CreateProvider();

        provider.Show("Title", "Message", persistent: true);

        state.Toasts[0].IsPersistent.Should().BeTrue();
    }

    [Fact]
    public void Show_NotPersistent_MarksAsEphemeral()
    {
        var (provider, state) = CreateProvider();

        provider.Show("Title", "Message", persistent: false);

        state.Toasts[0].IsPersistent.Should().BeFalse();
    }

    [Fact]
    public void Dismiss_RemovesToastFromState()
    {
        var (provider, state) = CreateProvider();
        var id = provider.Show("Title", "Message", persistent: true);

        provider.Dismiss(id);

        state.Toasts.Should().BeEmpty();
    }

    [Fact]
    public void EnforceCapacity_EvictsOldestWhenAtMax()
    {
        var (provider, state) = CreateProvider();
        provider.Show("First", "1", persistent: true);
        provider.Show("Second", "2", persistent: true);
        provider.Show("Third", "3", persistent: true);

        // Adding a 4th should evict the oldest (First)
        provider.Show("Fourth", "4", persistent: true);

        state.Toasts.Should().HaveCount(3);
        state.Toasts.Should().NotContain(t => t.Title == "First");
        state.Toasts.Should().Contain(t => t.Title == "Fourth");
    }

    [Fact]
    public void EnforceCapacity_EvictsEphemeralBeforePersistent()
    {
        var (provider, state) = CreateProvider();
        provider.Show("Persistent1", "1", persistent: true);
        provider.Show("Ephemeral1", "2", persistent: false);
        provider.Show("Persistent2", "3", persistent: true);

        // Adding a 4th should evict the ephemeral one first
        provider.Show("New", "4", persistent: true);

        state.Toasts.Should().HaveCount(3);
        state.Toasts.Should().NotContain(t => t.Title == "Ephemeral1");
        state.Toasts.Should().Contain(t => t.Title == "Persistent1");
        state.Toasts.Should().Contain(t => t.Title == "Persistent2");
        state.Toasts.Should().Contain(t => t.Title == "New");
    }

    [Fact]
    public void Dismiss_NonexistentId_DoesNotThrow()
    {
        var (provider, _) = CreateProvider();

        var act = () => provider.Dismiss(Guid.NewGuid());

        act.Should().NotThrow();
    }

    [Fact]
    public void Show_MultiplePersistent_AllMaxCapacity_EvictsOldest()
    {
        var (provider, state) = CreateProvider();
        provider.Show("P1", "1", persistent: true);
        provider.Show("P2", "2", persistent: true);
        provider.Show("P3", "3", persistent: true);

        provider.Show("P4", "4", persistent: true);

        state.Toasts.Should().HaveCount(3);
        state.Toasts[0].Title.Should().Be("P2");
    }
}
