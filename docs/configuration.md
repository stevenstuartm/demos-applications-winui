# Configuration

Configuration is split into focused, independently-registered types — no single global config object. All config types live in `Core.Configuration`.

## Host Config

| Type | Purpose |
|------|---------|
| `IHostConfig` | Read-only interface: `Stage` (AppStage) + `Environment` (AppEnvironment) |
| `HostConfig` | Plain class with `internal set` properties. Not observable — set once at startup. |
| `HostConfigResolver` | Static `Resolve()` method, reads env vars with defaults |

### Stage and Environment

- `AppStage` enum: `Local`, `Stage`, `Prod` — drives strategy selection (logging providers, error verbosity)
- `AppEnvironment` enum: `Dev`, `QA`, `Prod` — drives remote resource naming (e.g., "dev-notes-db")
- `APP_STAGE` and `APP_ENVIRONMENT` env vars, set via `launchSettings.json` (local) or deployment pipeline (Stage/Prod)
- Safe defaults: `Local` / `Dev`

### Registration

```csharp
var hostConfig = services.AddHostConfig();  // resolves and registers
// Returns IHostConfig for immediate use in ConfigureServices
```

## Logging Config

Logging is configured via layered `appsettings.json` files in the App project, read by Serilog's `ReadFrom.Configuration()`.

| File | Purpose |
|------|---------|
| `appsettings.json` | Base config: File sink, default level (Information), rolling params |
| `appsettings.Local.json` | Local dev: Debug level |
| `appsettings.Stage.json` | Stage overrides (add remote sinks here without code changes) |
| `appsettings.Prod.json` | Prod overrides (add remote sinks here without code changes) |

The resolved `AppStage` determines which override file is loaded. Environment variables can override any Serilog setting (e.g., `Serilog__MinimumLevel__Default=Warning`).

The log file directory defaults to `ApplicationData.Current.LocalFolder.Path/logs/` and can be overridden via the `APP_LOG_DIRECTORY` environment variable.

`IConfiguration` is built and consumed locally in `App.xaml.cs` — it is not registered in DI.

**Array override caveat:** When a stage JSON file includes a `WriteTo` array, it *replaces* (not appends to) the base array. A Prod file that adds a remote sink must also redeclare the File sink.

## Navigation Config

`NavigationConfig` — standalone singleton with fluent guard routing builder. See [navigation.md](navigation.md) for the guard system details.

```csharp
services.AddNavigationConfig(typeof(AllNotesPage), config =>
{
    config.Guard(NavigationGuardNames.IsAuthenticated).ForAll().Except(typeof(LoginPage));
});
```

## Domain Config Pattern

When a domain needs its own config:

1. Define config class in the domain project (e.g., `NotesConfig` in Notes)
2. Register via the domain's `AddX()` extension: `AddNotes(Action<NotesConfig>?)`
3. If the config needs cross-domain injection, follow `IFooConfig` (read-only) / `FooConfig` (mutable, `internal set`)

## Registration Extension Methods

All in `Core.Configuration.ConfigurationServiceCollectionExtensions`:

```csharp
services.AddHostConfig();                              // IHostConfig singleton
services.AddNavigationConfig(defaultPage, config => { ... });  // NavigationConfig singleton
```

## Logging Strategy

Configured in `App.xaml.cs` via `Serilog.Settings.Configuration`, driven by `AppStage`:

| Stage | Behavior |
|-------|----------|
| Local | `AddDebug()` (M.E.Logging provider) + Serilog from `appsettings.Local.json` (Debug level, rolling file) |
| Stage/Prod | Serilog from `appsettings.{Stage}.json` (Information level, rolling file, add remote sinks via JSON) |

Log files: `ApplicationData.Current.LocalFolder.Path/logs/`
Packages: `Serilog.Extensions.Logging`, `Serilog.Settings.Configuration`, `Serilog.Sinks.File`, `Microsoft.Extensions.Configuration.Json`, `Microsoft.Extensions.Configuration.EnvironmentVariables` (App project only)

## Global Exception Handling

Configured in `App.xaml.cs`:

- `UnhandledException` — logs critical via `ILogger`, shows toast, `e.Handled = true`
- `TaskScheduler.UnobservedTaskException` — logs critical, `e.SetObserved()`, dispatches toast via `DispatcherQueue`
- Error messages are inlined in App.xaml.cs (not in a shared UserMessages class)
