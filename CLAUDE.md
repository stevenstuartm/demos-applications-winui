# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build Commands

Build requires a platform specifier (no "AnyCPU" support):

```bash
dotnet build -p:Platform=ARM64          # ARM64
dotnet build -p:Platform=x64            # x64
dotnet build -p:Platform=x86            # x86
```

Run from the repo root (where `demos-applications-winui.sln` lives). There are no tests or linting tools configured.

## Architecture

Domain-driven WinUI 3 desktop app on .NET 10 with three projects:

- **App** (`demos-applications-winui/`) — WinExe shell, DI composition root, NavigationService implementation
- **Core** (`demos-applications-winui.Core/`) — Shared contracts only (navigation interfaces). No domain knowledge.
- **Notes** (`demos-applications-winui.Notes/`) — First domain module: models, services, view models, views

Dependency graph: **App → Notes → Core** (and App → Core directly). Domains never reference App or each other.

### Namespaces

Namespaces use underscores (matching the project naming convention):
- `demos_applications_winui` — App root
- `demos_applications_winui.Core.Navigation` — navigation contracts
- `demos_applications_winui.Notes.{Models,Services,ViewModels,Views}` — Notes domain
- `demos_applications_winui.Services` — NavigationService implementation

### DI and Lifecycle

All services, view models, and pages are registered as **singletons** in `App.xaml.cs`. Constructor injection is used everywhere — no service locator pattern outside the composition root. The only exception is `NavigationService`, which receives `IServiceProvider` to resolve pages by type.

### Navigation

Custom navigation system — does **not** use `Frame.Navigate()`. Instead, `NavigationService` sets `Frame.Content` directly with DI-resolved singleton page instances. Key design:

- Entry-based back stack: `Stack<NavigationEntry(Type, Parameter)>` stores page types, not instances
- `NavigationContext(Parameter, Mode)` tells VMs why they're being navigated to (`New` vs `Back`)
- Pages and VMs implement `INavigable` — pages delegate to their VMs
- `NavigateAndReplace<T>()` clears the back stack (designed for auth flows)
- `INavigationGuard` provides pluggable navigation interception (redirect or block)
- `_navigationInProgress` flag prevents concurrent navigation

### MVVM Pattern

Uses CommunityToolkit.Mvvm (`ObservableObject`, `[ObservableProperty]`, `[RelayCommand]`). Views are thin code-behind delegates — all logic lives in view models.

## Conventions

- New domains should follow the Notes project structure: separate class library with Models/, Services/, ViewModels/, Views/ folders, referencing only Core
- Register all new pages, VMs, and services as singletons in `App.xaml.cs`
- New pages that participate in navigation must implement `INavigable` (delegate to VM)
- Navigation guards implementing `INavigationGuard` are injected as `IEnumerable<INavigationGuard>` into NavigationService
