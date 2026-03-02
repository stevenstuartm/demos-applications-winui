# Navigation

## Overview

Custom navigation system that does **not** use `Frame.Navigate()`. Instead, `NavigationService` sets `Frame.Content` directly with DI-resolved singleton page instances.

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
| `NavigateToAsync<TPage>(parameter?)` | Checks guards, pushes current page to back stack, resolves and displays new page |
| `NavigateAndReplaceAsync<TPage>(parameter?)` | Checks guards, clears back stack, resolves and displays new page. Used for auth flows. |
| `NavigateToDefaultAsync()` | Navigates to `NavigationConfig.DefaultPage` with cleared back stack |
| `GoBackAsync()` | Pops from back stack, resolves and displays previous page |
| `SetFrame(Frame)` | Called once from MainWindow constructor |

### Back Stack

Entry-based: `Stack<NavigationStackEntry(Type PageType, object? Parameter)>` stores page types and parameters, not instances. Pages are re-resolved from DI on back navigation (they're singletons, so same instance).

### Concurrency Guard

`_navigationInProgress` flag prevents concurrent navigation. All public methods check this flag and return early if navigation is already in progress.

### Lifecycle

1. `CheckCanNavigateFromAsync()` — asks current page if it's OK to leave (e.g., unsaved changes dialog)
2. `CheckGuards()` — evaluates guards, may block or redirect
3. `OnNavigatedFrom()` on current page
4. Push current to back stack (unless replacing)
5. Resolve new page from DI, set as `Frame.Content`
6. `OnNavigatedToAsync(NavigationContext)` on new page

## INavigable (Toolkit.Navigation)

Pages and VMs implement this to participate in the navigation lifecycle:

```csharp
public interface INavigable
{
    Task OnNavigatedToAsync(NavigationContext context);
    void OnNavigatedFrom();
    Task<bool> CanNavigateFromAsync() => Task.FromResult(true);  // default impl
}
```

- `NavigationContext(object? Parameter, NavigationMode Mode)` — Mode is `New` or `Back`
- Pages delegate to their VMs: `public Task OnNavigatedToAsync(NavigationContext context) => ViewModel.OnNavigatedToAsync(context);`
- `CanNavigateFromAsync()` — return `false` to block navigation (e.g., unsaved changes confirmation)

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
