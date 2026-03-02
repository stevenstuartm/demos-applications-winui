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

Domain-driven WinUI 3 desktop app on .NET 10 with four projects:

- **App** (`demos-applications-winui/`) — WinExe shell, DI composition root
- **Core** (`demos-applications-winui.Core/`) — Shared contracts, navigation infrastructure, and configuration. No domain knowledge.
- **Auth** (`demos-applications-winui.Auth/`) — Authentication domain: login page, auth service, auth state, navigation guard
- **Notes** (`demos-applications-winui.Notes/`) — Notes domain: models, services, view models, views

Dependency graph: **App → Auth → Core**, **App → Notes → Core** (and App → Core directly). Domains never reference App or each other.

### Namespaces

Namespaces use underscores (matching the project naming convention):
- `demos_applications_winui` — App root
- `demos_applications_winui.Core.Navigation` — navigation contracts and NavigationService implementation
- `demos_applications_winui.Core.Configuration` — IHostConfig/HostConfig, NavigationConfig, LoggingConfig, guard routing builders
- `demos_applications_winui.Core.Auth` — IAuthState contract
- `demos_applications_winui.Auth.{Models,Services,Navigation,ViewModels,Views}` — Auth domain
- `demos_applications_winui.Notes.{Models,Services,ViewModels,Views}` — Notes domain

### DI and Lifecycle

All services, view models, and pages are registered as **singletons** in `App.xaml.cs`. Constructor injection is used everywhere — no service locator pattern outside the composition root. The only exception is `NavigationService`, which receives `IServiceProvider` to resolve pages by type.

### Navigation

Custom navigation system — does **not** use `Frame.Navigate()`. Instead, `NavigationService` sets `Frame.Content` directly with DI-resolved singleton page instances. Key design:

- Entry-based back stack: `Stack<NavigationStackEntry(Type, Parameter)>` stores page types, not instances
- `NavigationContext(Parameter, Mode)` tells VMs why they're being navigated to (`New` vs `Back`)
- State/service separation: `INavigationState` (read-only, bindable) + `INavigationService` (actions only)
- Pages and VMs implement `INavigable` — pages delegate to their VMs
- `NavigateAndReplace<T>()` clears the back stack (designed for auth flows)
- `_navigationInProgress` flag prevents concurrent navigation

### Navigation Guards

Guards intercept navigation to block or redirect. The system uses a **config-based routing model** (no reflection, AOT-safe) with a fluent API configured in `App.xaml.cs`:

```csharp
navigationConfig
    .Guard(NavigationGuardNames.IsAuthenticated).ForAll().Except(typeof(LoginPage))
    .Guard(NavigationGuardNames.AdminRole).ForPages(typeof(AdminPage));
```

Key design:
- **Guard routing is centralized** in `NavigationConfig` — a single place to audit which guards apply to which pages
- **Global guards** (`.ForAll()`) apply to all pages with optional `.Except()` exclusions — new pages are protected by default
- **Targeted guards** (`.ForPages()`) apply only to listed pages — for role checks, dirty-form protection, etc.
- Guards implement `INavigationGuard` with a `Name` property matched against `NavigationGuardNames` constants
- `NavigationGuardContext` provides target page, source page, and parameter to guard logic
- Guard-to-page resolution is cached per page type in `NavigationConfig.GetGuardsForPage()`
- Fluent builders (`GuardBuilder` → `GlobalGuardBuilder`) enforce valid combinations at compile time with `internal` constructors

### Configuration

Configuration is split into focused, independently-registered types — no single global config object.

| Type | Purpose | Registered As |
|------|---------|---------------|
| `IHostConfig` / `HostConfig` | Stage + Environment | `IHostConfig` (read-only) + `HostConfig` (mutable, `internal set`) |
| `NavigationConfig` | Guard routing + default page | concrete singleton |
| `LoggingConfig` | Serilog file settings | concrete singleton |

**Stage/Environment resolution:** `HostConfigResolver.Resolve()` reads environment variables `APP_STAGE` and `APP_ENVIRONMENT` with safe defaults (`Local`/`Dev`). Set via `launchSettings.json` for local dev, deployment pipeline for Stage/Prod.

**Async startup configs:** `IStartupConfigProvider` is the extensibility hook for API-sourced configs. Implementations are resolved and called in `OnLaunched` before first navigation. Currently no providers are registered.

**Domain configs:** When a domain needs its own config, define it in the domain project (e.g., `NotesConfig` in Notes), register via `AddNotes(Action<NotesConfig>?)`. Follow the same `IFooConfig` (read-only) / `FooConfig` (mutable) pattern if the config needs cross-domain injection.

### MVVM Pattern

Uses CommunityToolkit.Mvvm (`ObservableObject`, `[ObservableProperty]`, `[RelayCommand]`). Views are thin code-behind delegates — all logic lives in view models. State interfaces (`INavigationState`, `IAuthState`) are read-only and bindable; mutable implementations use `internal set`.

## Conventions

- New domains should follow the Notes/Auth project structure: separate class library with Models/, Services/, ViewModels/, Views/ folders, referencing only Core
- Register all new pages, VMs, and services as singletons in `App.xaml.cs`
- New pages that participate in navigation must implement `INavigable` (delegate to VM)
- Navigation guards: implement `INavigationGuard` in the domain, register in DI, add a `NavigationGuardNames` constant in Core, and configure routing in `App.xaml.cs`
- State/service separation: `IFooState` (Core, read-only, INPC) + `FooState` (domain, mutable, `internal set`) + `IFooService` (actions)
- No implicit usings — all `System.*` usings must be explicit
