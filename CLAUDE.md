# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository. Detailed reference docs live in `docs/` — read the relevant file before making changes in that area.

## Build Commands

```bash
dotnet build -p:Platform=ARM64          # ARM64
dotnet build -p:Platform=x64            # x64
dotnet build -p:Platform=x86            # x86
```

Run from the repo root (where `demos-applications-winui.sln` lives). There are no tests or linting tools configured.

## Architecture

Domain-driven WinUI 3 desktop app on .NET 10 with five projects:

| Project | Role | Dependencies |
|---------|------|-------------|
| **App** (`demos-applications-winui/`) | WinExe shell, DI composition root | Toolkit, Core, Auth, Notes |
| **Core** (`demos-applications-winui.Core/`) | Logical authority: security policy (guards), configuration, identity contracts. **Pure .NET — zero UI packages.** | DI only |
| **Toolkit** (`demos-applications-winui.Toolkit/`) | Common UI library: navigation, platform providers, toast component. Self-contained — owns interfaces and implementations. | Core |
| **Auth** (`demos-applications-winui.Auth/`) | Authentication domain: login, auth service/state, navigation guard | Toolkit, Core |
| **Notes** (`demos-applications-winui.Notes/`) | Notes domain: models, services, VMs, views, WebView2 editor | Toolkit, Core |

Dependency graph: **App → Auth/Notes → Toolkit → Core**. Domains never reference App or each other.

### Namespaces

Namespaces use underscores (matching the project naming convention):

- `demos_applications_winui` — App root
- `demos_applications_winui.Core.Navigation` — guard interfaces and guard names (security policy only)
- `demos_applications_winui.Core.Configuration` — IHostConfig, NavigationConfig, LoggingConfig, guard routing builders
- `demos_applications_winui.Core.Auth` — IAuthState contract
- `demos_applications_winui.Toolkit.Navigation` — INavigationService, NavigationService, INavigable, NavigationState, NavigationContext
- `demos_applications_winui.Toolkit.Platform` — IToastProvider, IDialogProvider, IFilePickerProvider, ICameraProvider, IWindowHandleProvider
- `demos_applications_winui.Toolkit.Toast` — ToastPresenter, ToastState, ToastItem (component internals)
- `demos_applications_winui.Toolkit.Providers` — DialogProvider, FilePickerProvider, CameraProvider, WindowHandleProvider
- `demos_applications_winui.Toolkit.Configuration` — AddNavigation(), AddToast(), AddProviders()
- `demos_applications_winui.Auth.{Models,Services,Navigation,ViewModels,Views}` — Auth domain
- `demos_applications_winui.Notes.{Models,Services,ViewModels,Views}` — Notes domain

### Key Architectural Boundaries

- **Core** = logical authority. No UI packages. Guards, config, identity contracts, enums. Could be referenced by a non-UI project.
- **Toolkit** = common UI library. Owns both interfaces and implementations for navigation, platform providers, and components. Any project with UI depends on this.
- **Domains** reference both Core (guard names, config) and Toolkit (navigation, providers). They contain models, services, VMs, and views together.

### DI and Lifecycle

All services, view models, and pages are registered as **singletons** in `App.xaml.cs`. Constructor injection is used everywhere — no service locator pattern outside the composition root. The only exception is `NavigationService`, which receives `IServiceProvider` to resolve pages by type.

### MVVM Pattern

Uses CommunityToolkit.Mvvm (`ObservableObject`, `[ObservableProperty]`, `[RelayCommand]`). Views are thin code-behind delegates — all logic lives in view models. State interfaces (`INavigationState`, `IAuthState`) are read-only and bindable; mutable implementations use `internal set`.

## Conventions

- New domains: separate class library with Models/, Services/, ViewModels/, Views/ folders, referencing Toolkit and Core
- Register all new pages, VMs, and services as singletons in `App.xaml.cs`
- New pages must implement `INavigable` (from `Toolkit.Navigation`) — pages delegate to their VMs
- Navigation guards: implement `INavigationGuard` (Core), register in DI, add `NavigationGuardNames` constant (Core), configure routing in `App.xaml.cs`
- State/service separation: `IFooState` (read-only, INPC) + `FooState` (mutable, `internal set`) + `IFooService` (actions)
- No implicit usings — all `System.*` usings must be explicit
- "Provider" suffix for platform wrappers (not "Service") — e.g., `IDialogProvider`, `CameraProvider`
- UserMessages: domain-specific message constants live in the consuming domain project, not in Core or Toolkit

## Reference Docs

Read the relevant doc before modifying code in that area:

- **[docs/navigation.md](docs/navigation.md)** — Navigation service, guards, INavigable lifecycle, guard config API
- **[docs/toolkit.md](docs/toolkit.md)** — Toast component, platform providers, DI registration patterns
- **[docs/configuration.md](docs/configuration.md)** — HostConfig, LoggingConfig, resolvers, startup providers, domain config pattern
- **[docs/winui-gotchas.md](docs/winui-gotchas.md)** — Platform-specific issues and workarounds
