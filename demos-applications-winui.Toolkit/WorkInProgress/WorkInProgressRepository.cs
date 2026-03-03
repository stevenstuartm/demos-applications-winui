using System;
using System.Collections.Generic;
using System.Linq;

namespace demos_applications_winui.Toolkit.WorkInProgress;

/// <summary>
/// Singleton repository that manages all WIP sessions by key. Sessions are created
/// on first <see cref="Get{TData}"/> call and stored until explicitly removed.
/// </summary>
internal class WorkInProgressRepository : IWorkInProgressRepository
{
    private readonly Dictionary<string, IWorkInProgress> _sessions = new();

    public IWorkInProgress<TData> Get<TData>(string key) where TData : class
    {
        if (_sessions.TryGetValue(key, out var existing))
        {
            if (existing is IWorkInProgress<TData> typed)
                return typed;

            throw new InvalidOperationException(
                $"WIP session '{key}' exists but is not of type {typeof(TData).Name}");
        }

        var session = new WorkInProgress<TData>();
        _sessions[key] = session;
        return session;
    }

    public bool Has(string key) => _sessions.ContainsKey(key);

    public bool HasAnyPendingWork() => _sessions.Values.Any(s => s.HasPendingWork);

    public void Remove(string key)
    {
        if (_sessions.Remove(key, out var session))
            session.Clear();
    }

    public void RemoveAll()
    {
        foreach (var session in _sessions.Values)
            session.Clear();
        _sessions.Clear();
    }
}
