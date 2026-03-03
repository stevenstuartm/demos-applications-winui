# Navigation

## Overview

Custom navigation system that does **not** use `Frame.Navigate()`. Instead, `NavigationService` creates fresh DI-resolved page instances and sets `Frame.Content` directly. Previous pages are disposed.

Navigation mechanism lives in **Toolkit.Navigation**. Guard policy lives in **Core.Navigation**.

## NavigationService (Toolkit.Navigation)

Primary constructor dependencies:
- `IServiceProvider` — resolves pages by type
- `IEnumerable<INavigationGuard>` — all registered guards (from Core.Navigation)
- `NavigationState` — mutable state (Toolkit.Navigation)
- `NavigationConfig` — guard routing config (Core.Configuration)
- `ILogger<NavigationService>`

### Methods

| Method | Behavior |
|--------|----------|
| `NavigateToAsync<TPage>(parameter?)` | Checks guards, disposes current page, pushes entry to back stack, resolves fresh page, calls `InitializeAsync` |
| `NavigateAndReplaceAsync<TPage>(parameter?)` | Checks guards (skips `CanNavigateFromAsync`), disposes current page, clears back stack, resolves fresh page. Used for auth flows. |
| `NavigateToDefaultAsync()` | Navigates to `NavigationConfig.DefaultPage` with cleared back stack |
| `GoBackAsync()` | Disposes current page, pops entry from back stack, resolves fresh page, calls `InitializeAsync(entry.Parameter)` |
| `SetFrame(INavigationFrame)` | Called once from MainWindow constructor |

### Transient Pages

Every navigation creates a fresh page instance from DI. The previous page is disposed via `IDisposable` if it implements it. The back stack stores `(Type, Parameter)` entries — not live instances.

This means:
- Pages/VMs can use constructor defaults and fields without manual reset logic
- No `OnNavigatedFrom` — use `IDisposable.Dispose()` for cleanup (cancel commands, unsub events)
- No `OnNavigatedToAsync(NavigationContext)` — use `InitializeAsync(object? parameter)` instead
- Back navigation creates a completely fresh page, re-initialized with the stored parameter

### Back Stack

Entry-based: `Stack<NavigationStackEntry(Type PageType, object? Parameter)>`. On back navigation, the entry is popped, a fresh page is resolved from DI, and `InitializeAsync(entry.Parameter)` is called.

### Navigation Parameters

**Parameters are load hints, not mutable working state.** The back stack stores the original parameter reference. Pages must treat parameters as "what to load" — an ID, a model snapshot to copy from, or `null` — and copy what they need into local fields during `InitializeAsync`. Do not bind directly to a parameter object or assume it remains unchanged across navigations.

- **IDs and null** — always safe. The page fetches current state from the service.
- **Model objects** — safe when the page copies fields into its own state during `InitializeAsync` (current pattern). Unsafe if the page holds a live reference that other pages also mutate.
- **Multi-page editing flows** — use the WIP repository. The parameter identifies which WIP session to resume; the WIP session holds the authoritative editing state.

### Concurrency Guard

`_navigationInProgress` flag prevents concurrent navigation. All public methods check this flag and return early if navigation is already in progress.

### Lifecycle

1. `CheckCanNavigateFromAsync()` — asks current page if it's OK to leave (e.g., unsaved changes dialog)
2. `CheckGuards()` — evaluates guards, may block or redirect
3. `DisposeIfNeeded()` on current page — calls `IDisposable.Dispose()` if implemented
4. Push `(Type, Parameter)` to back stack (unless replacing/clearing)
5. Resolve fresh page from DI, set as `Frame.Content`
6. `InitializeAsync(parameter)` on new page

**Note**: `NavigateAndReplaceAsync` skips step 1 (`CanNavigateFromAsync`) — this is intentional for auth redirect flows where the user should not be prompted.

## INavigable (Toolkit.Navigation)

Pages implement this to participate in the navigation lifecycle:

```csharp
public interface INavigable
{
    Task InitializeAsync(object? parameter) => Task.CompletedTask;   // default: no-op
    Task<bool> CanNavigateFromAsync() => Task.FromResult(true);      // default: allow
}
```

- Pages delegate to their VMs: `public Task InitializeAsync(object? parameter) => ViewModel.InitializeAsync(parameter);`
- `CanNavigateFromAsync()` — return `false` to block navigation (e.g., unsaved changes confirmation)
- For cleanup, implement `IDisposable` on the page/VM (cancel commands, unsubscribe events)

## Navigation Guards (Core.Navigation)

Guards are security policy — they live in Core because they express logical authority, not UI mechanism.

### Guard Interface

```csharp
public interface INavigationGuard
{
    string Name { get; }
    NavigationGuardResult CheckNavigation(NavigationGuardContext context);
}
```

- `NavigationGuardContext(Type TargetPageType, Type? SourcePageType, object? Parameter)`
- `NavigationGuardResult.Allow` — static allow
- `NavigationGuardResult.RedirectTo(typeof(LoginPage))` — redirect to another page

### Guard Routing (Core.Configuration)

Configured via fluent API in `App.xaml.cs` on `NavigationConfig`:

```csharp
services.AddNavigationConfig(typeof(AllNotesPage), config =>
{
    config.Guard(NavigationGuardNames.IsAuthenticated).ForAll().Except(typeof(LoginPage));
});
```

- **Global guards** (`.ForAll()`) — apply to all pages with optional `.Except()` exclusions. New pages are protected by default.
- **Targeted guards** (`.ForPages()`) — apply only to listed pages. For role checks, feature gates, etc.
- Guard names are string constants in `NavigationGuardNames` (Core.Navigation)
- Guard-to-page resolution is cached per page type in `NavigationConfig.GetGuardsForPage()` (public for cross-assembly access)
- Fluent builders (`GuardBuilder` → `GlobalGuardBuilder`) enforce valid combinations at compile time

### Adding a New Guard

1. Add constant to `NavigationGuardNames` in Core
2. Implement `INavigationGuard` in the domain project
3. Register as `INavigationGuard` singleton in the domain's `ServiceCollectionExtensions`
4. Configure routing in `App.xaml.cs` via the fluent API

## NavigationState (Toolkit.Navigation)

Read-only state for UI binding:

```csharp
public interface INavigationState : INotifyPropertyChanged
{
    bool CanGoBack { get; }
    Type? CurrentPageType { get; }
    IReadOnlyList<NavigationStackEntry>? BackStack { get; }
}
```

`NavigationState` (mutable, `internal set`) uses `ObservableObject` + `[ObservableProperty]` from CommunityToolkit.Mvvm. Updated by `NavigationService` after each navigation.

## DI Registration

```csharp
// In Toolkit.Configuration.ToolkitServiceCollectionExtensions
services.AddNavigation();  // NavigationState, INavigationState, INavigationService

// In App.xaml.cs — guard routing
services.AddNavigationConfig(typeof(AllNotesPage), config => { ... });
```
