using System;
using System.Collections.Generic;

namespace demos_applications_winui.Core.Configuration;

/// <summary>
/// Internal representation of a guard-to-page mapping, built by the fluent API.
/// Global guards apply to all pages except those in <see cref="ExcludedPages"/>.
/// Non-global guards apply only to pages in <see cref="IncludedPages"/>.
/// </summary>
internal record GuardRegistration
{
    public required string GuardName { get; init; }
    public bool IsGlobal { get; init; }
    public HashSet<Type> ExcludedPages { get; set; } = [];
    public HashSet<Type> IncludedPages { get; init; } = [];
}
