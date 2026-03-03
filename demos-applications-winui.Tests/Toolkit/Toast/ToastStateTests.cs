using System;
using FluentAssertions;
using demos_applications_winui.Toolkit.Platform;
using demos_applications_winui.Toolkit.Toast;
using Xunit;

namespace demos_applications_winui.Tests.Toolkit.Toast;

public class ToastStateTests
{
    private static ToastState CreateState() => new();

    private static ToastItem CreateToast(string title = "Test") => new()
    {
        Id = Guid.NewGuid(),
        Title = title,
        Message = "Message",
        Severity = ToastSeverity.Informational,
        IsPersistent = false
    };

    [Fact]
    public void Initial_ToastsIsEmpty()
    {
        var state = CreateState();

        state.Toasts.Should().BeEmpty();
    }

    [Fact]
    public void Add_InsertsToast()
    {
        var state = CreateState();
        var toast = CreateToast("Hello");

        state.Add(toast);

        state.Toasts.Should().HaveCount(1);
        state.Toasts[0].Title.Should().Be("Hello");
    }

    [Fact]
    public void Add_Multiple_MaintainsOrder()
    {
        var state = CreateState();

        state.Add(CreateToast("First"));
        state.Add(CreateToast("Second"));

        state.Toasts.Should().HaveCount(2);
        state.Toasts[0].Title.Should().Be("First");
        state.Toasts[1].Title.Should().Be("Second");
    }

    [Fact]
    public void Remove_ExistingId_RemovesAndReturnsTrue()
    {
        var state = CreateState();
        var toast = CreateToast();
        state.Add(toast);

        var result = state.Remove(toast.Id);

        result.Should().BeTrue();
        state.Toasts.Should().BeEmpty();
    }

    [Fact]
    public void Remove_UnknownId_ReturnsFalse()
    {
        var state = CreateState();

        var result = state.Remove(Guid.NewGuid());

        result.Should().BeFalse();
    }

    [Fact]
    public void Toasts_IsReadOnlyProjection()
    {
        var state = CreateState();
        var toast = CreateToast();
        state.Add(toast);

        // ReadOnlyObservableCollection reflects changes to the backing collection
        state.Toasts.Should().HaveCount(1);

        state.Remove(toast.Id);
        state.Toasts.Should().BeEmpty();
    }
}
