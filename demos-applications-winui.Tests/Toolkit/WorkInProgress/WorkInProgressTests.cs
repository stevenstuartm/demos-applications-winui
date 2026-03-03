using System;
using System.Collections.Generic;
using System.ComponentModel;
using FluentAssertions;
using demos_applications_winui.Toolkit.WorkInProgress;
using Xunit;

namespace demos_applications_winui.Tests.Toolkit.WorkInProgress;

public class WorkInProgressTests
{
    private class TestData
    {
        public string? Name { get; set; }
    }

    private class OtherData
    {
        public int Value { get; set; }
    }

    // --- Session lifecycle ---

    [Fact]
    public void Begin_SetsData_ResetsIsDirty()
    {
        var repo = new WorkInProgressRepository();
        var wip = repo.Get<TestData>("test");
        var data = new TestData { Name = "hello" };

        wip.Begin(data);

        wip.Data.Should().BeSameAs(data);
        wip.IsDirty.Should().BeFalse();
    }

    [Fact]
    public void MarkDirty_SetsIsDirtyTrue()
    {
        var repo = new WorkInProgressRepository();
        var wip = repo.Get<TestData>("test");
        wip.Begin(new TestData());

        wip.MarkDirty();

        wip.IsDirty.Should().BeTrue();
    }

    [Fact]
    public void MarkClean_SetsIsDirtyFalse_KeepsData()
    {
        var repo = new WorkInProgressRepository();
        var wip = repo.Get<TestData>("test");
        var data = new TestData { Name = "keep" };
        wip.Begin(data);
        wip.MarkDirty();

        wip.MarkClean();

        wip.IsDirty.Should().BeFalse();
        wip.Data.Should().BeSameAs(data);
    }

    [Fact]
    public void Clear_RemovesData_ResetsIsDirty()
    {
        var repo = new WorkInProgressRepository();
        var wip = repo.Get<TestData>("test");
        wip.Begin(new TestData());
        wip.MarkDirty();

        wip.Clear();

        wip.Data.Should().BeNull();
        wip.IsDirty.Should().BeFalse();
    }

    // --- HasPendingWork ---

    [Fact]
    public void HasPendingWork_FalseWhenNoData()
    {
        var repo = new WorkInProgressRepository();
        var wip = repo.Get<TestData>("test");

        wip.HasPendingWork.Should().BeFalse();
    }

    [Fact]
    public void HasPendingWork_FalseWhenClean()
    {
        var repo = new WorkInProgressRepository();
        var wip = repo.Get<TestData>("test");
        wip.Begin(new TestData());

        wip.HasPendingWork.Should().BeFalse();
    }

    [Fact]
    public void HasPendingWork_TrueWhenDirtyWithData()
    {
        var repo = new WorkInProgressRepository();
        var wip = repo.Get<TestData>("test");
        wip.Begin(new TestData());
        wip.MarkDirty();

        wip.HasPendingWork.Should().BeTrue();
    }

    // --- Repository ---

    [Fact]
    public void Get_CreatesNewSession_WhenKeyNotFound()
    {
        var repo = new WorkInProgressRepository();

        var wip = repo.Get<TestData>("new-key");

        wip.Should().NotBeNull();
        wip.Data.Should().BeNull();
        wip.IsDirty.Should().BeFalse();
    }

    [Fact]
    public void Get_ReturnsSameSession_ForSameKey()
    {
        var repo = new WorkInProgressRepository();
        var first = repo.Get<TestData>("same");
        first.Begin(new TestData { Name = "persisted" });

        var second = repo.Get<TestData>("same");

        second.Should().BeSameAs(first);
        second.Data!.Name.Should().Be("persisted");
    }

    [Fact]
    public void Get_ThrowsOnTypeMismatch()
    {
        var repo = new WorkInProgressRepository();
        repo.Get<TestData>("shared-key");

        var act = () => repo.Get<OtherData>("shared-key");

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*shared-key*OtherData*");
    }

    [Fact]
    public void Has_ReturnsTrueForExistingKey()
    {
        var repo = new WorkInProgressRepository();
        repo.Get<TestData>("exists");

        repo.Has("exists").Should().BeTrue();
        repo.Has("missing").Should().BeFalse();
    }

    [Fact]
    public void HasAnyPendingWork_TrueWhenAnySessionDirty()
    {
        var repo = new WorkInProgressRepository();
        var clean = repo.Get<TestData>("clean");
        clean.Begin(new TestData());

        var dirty = repo.Get<OtherData>("dirty");
        dirty.Begin(new OtherData());
        dirty.MarkDirty();

        repo.HasAnyPendingWork().Should().BeTrue();
    }

    [Fact]
    public void HasAnyPendingWork_FalseWhenAllClean()
    {
        var repo = new WorkInProgressRepository();
        var wip = repo.Get<TestData>("clean");
        wip.Begin(new TestData());

        repo.HasAnyPendingWork().Should().BeFalse();
    }

    [Fact]
    public void Remove_ClearsAndRemovesSession()
    {
        var repo = new WorkInProgressRepository();
        var wip = repo.Get<TestData>("doomed");
        wip.Begin(new TestData { Name = "gone" });
        wip.MarkDirty();

        repo.Remove("doomed");

        repo.Has("doomed").Should().BeFalse();
        wip.Data.Should().BeNull();
        wip.IsDirty.Should().BeFalse();
    }

    [Fact]
    public void Remove_NonExistentKey_DoesNotThrow()
    {
        var repo = new WorkInProgressRepository();

        var act = () => repo.Remove("ghost");

        act.Should().NotThrow();
    }

    [Fact]
    public void RemoveAll_ClearsAllSessions()
    {
        var repo = new WorkInProgressRepository();
        var a = repo.Get<TestData>("a");
        a.Begin(new TestData());
        a.MarkDirty();
        var b = repo.Get<OtherData>("b");
        b.Begin(new OtherData());
        b.MarkDirty();

        repo.RemoveAll();

        repo.Has("a").Should().BeFalse();
        repo.Has("b").Should().BeFalse();
        a.Data.Should().BeNull();
        b.Data.Should().BeNull();
    }

    // --- INPC ---

    [Fact]
    public void PropertyChanged_FiresForData_IsDirty_HasPendingWork()
    {
        var repo = new WorkInProgressRepository();
        var wip = repo.Get<TestData>("inpc");
        var changed = new List<string>();
        ((INotifyPropertyChanged)wip).PropertyChanged += (_, e) =>
        {
            if (e.PropertyName is not null)
                changed.Add(e.PropertyName);
        };

        wip.Begin(new TestData());
        changed.Should().Contain(nameof(IWorkInProgress<TestData>.Data));
        changed.Should().Contain(nameof(IWorkInProgress.HasPendingWork));
        changed.Clear();

        wip.MarkDirty();
        changed.Should().Contain(nameof(IWorkInProgress<TestData>.IsDirty));
        changed.Should().Contain(nameof(IWorkInProgress.HasPendingWork));
        changed.Clear();

        wip.MarkClean();
        changed.Should().Contain(nameof(IWorkInProgress<TestData>.IsDirty));
        changed.Clear();

        wip.Clear();
        changed.Should().Contain(nameof(IWorkInProgress<TestData>.Data));
    }
}
