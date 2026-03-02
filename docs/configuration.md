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

| Type | Purpose |
|------|---------|
| `LoggingConfig` | Serilog file settings: LogDirectory, FileNameTemplate, FileSizeLimitBytes, RetainedFileCountLimit |
| `LoggingConfigResolver` | Static `Resolve()` method |

Properties are nullable — App.xaml.cs provides fallback values when consuming:
```csharp
var logDir = loggingConfig.LogDirectory
    ?? Path.Combine(ApplicationData.Current.LocalFolder.Path, "logs");
```

## Navigation Config

`NavigationConfig` — standalone singleton with fluent guard routing builder. See [navigation.md](navigation.md) for the guard system details.

```csharp
services.AddNavigationConfig(typeof(AllNotesPage), config =>
{
    config.Guard(NavigationGuardNames.IsAuthenticated).ForAll().Except(typeof(LoginPage));
});
```

## Async Startup Configs

`IStartupConfigProvider` is the extensibility hook for API-sourced configs that need async loading:

```csharp
public interface IStartupConfigProvider
{
    int Order => 0;  // lower runs first
    Task LoadAsync(CancellationToken cancellationToken = default);
}
```

Implementations are resolved and called in `App.OnLaunched` before first navigation. Currently no providers are registered.

Implementations should be resilient: cache last-known-good values, timeout gracefully, never throw.

## Domain Config Pattern

When a domain needs its own config:

1. Define config class in the domain project (e.g., `NotesConfig` in Notes)
2. Register via the domain's `AddX()` extension: `AddNotes(Action<NotesConfig>?)`
3. If the config needs cross-domain injection, follow `IFooConfig` (read-only) / `FooConfig` (mutable, `internal set`)

## Registration Extension Methods

All in `Core.Configuration.ConfigurationServiceCollectionExtensions`:

```csharp
services.AddHostConfig();                              // IHostConfig singleton
services.AddLoggingConfig();                           // LoggingConfig singleton
services.AddNavigationConfig(defaultPage, config => { ... });  // NavigationConfig singleton
```

## Logging Strategy

Configured in `App.xaml.cs`, driven by `AppStage`:

| Stage | Providers |
|-------|-----------|
| Local | `AddDebug()` + Serilog rolling file (Debug level) |
| Stage/Prod | Serilog rolling file only (Information level) + placeholder for remote sink |

Log files: `ApplicationData.Current.LocalFolder.Path/logs/`
Packages: `Serilog.Extensions.Logging` + `Serilog.Sinks.File` (App project only)

## Global Exception Handling

Configured in `App.xaml.cs`:

- `UnhandledException` — logs critical via `ILogger`, shows toast, `e.Handled = true`
- `TaskScheduler.UnobservedTaskException` — logs critical, `e.SetObserved()`, dispatches toast via `DispatcherQueue`
- Error messages are inlined in App.xaml.cs (not in a shared UserMessages class)
