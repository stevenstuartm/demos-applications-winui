# Toolkit

The Toolkit project (`demos-applications-winui.Toolkit/`) is the self-contained common UI library. It owns both interfaces and implementations for navigation, platform providers, and reusable components.

**Depends on:** Core (for guard types, NavigationConfig)
**Referenced by:** Auth, Notes, App

## Project Structure

```
Toolkit/
  Navigation/         — INavigationService, NavigationService, INavigable, NavigationState, etc.
  Platform/           — Provider interfaces: IToastProvider, IDialogProvider, IFilePickerProvider, etc.
  Toast/              — ToastPresenter UserControl + internal types (ToastState, ToastItem, ToastProvider)
  Providers/          — Platform implementations: DialogProvider, FilePickerProvider, CameraProvider, WindowHandleProvider
  Configuration/      — ServiceCollectionExtensions: AddNavigation(), AddToast(), AddProviders()
```

## Toast Component (Toolkit.Toast + Toolkit.Platform)

Self-contained toast notification system. The component owns its internal types — only `IToastProvider` and `ToastSeverity` are in `Toolkit.Platform` for domain consumption.

### Architecture

| Type | Namespace | Visibility | Purpose |
|------|-----------|-----------|---------|
| `IToastProvider` | Toolkit.Platform | Public | Interface domains inject to show toasts |
| `ToastSeverity` | Toolkit.Platform | Public | Informational, Success, Warning, Error |
| `ToastPresenter` | Toolkit.Toast | Public (DI) | UserControl with ItemsControl + InfoBar template |
| `ToastState` / `IToastState` | Toolkit.Toast | Public (DI) | Observable collection of active toasts |
| `ToastItem` | Toolkit.Toast | Public | Data model: Id, Title, Message, Severity, IsPersistent |
| `ToastProvider` | Toolkit.Toast | Public (DI) | Implementation: show, dismiss, auto-dismiss, capacity enforcement |

### Behavior

- Max 3 visible toasts — oldest ephemeral evicted first, then oldest persistent
- Non-persistent toasts auto-dismiss after 3 seconds via `CancellationTokenSource`
- Manual dismiss cancels the auto-dismiss timer
- `ToastPresenter` uses DI constructor injection — added to MainWindow programmatically (not in XAML) because WinUI XAML requires parameterless constructors

### Integration

```csharp
// MainWindow constructor
Grid.SetRow(toastPresenter, 1);
RootGrid.Children.Add(toastPresenter);

// Any VM or service
toastProvider.ShowSuccess("Saved", "Note saved successfully.");
toastProvider.ShowError("Error", "Something went wrong.", persistent: true);
```

### ConvertSeverity

`ToastPresenter.ConvertSeverity(ToastSeverity)` maps `ToastSeverity` enum to WinUI `InfoBarSeverity` via cast — the enum values are aligned by design.

## Platform Providers (Toolkit.Platform + Toolkit.Providers)

**Naming convention:** "Provider" suffix for platform wrappers (not "Service"). These are thin wrappers around WinUI APIs.

| Interface (Toolkit.Platform) | Implementation (Toolkit.Providers) | Purpose |
|------------------------------|--------------------------------------|---------|
| `IDialogProvider` | `DialogProvider` | ContentDialog wrapper. Requires `SetXamlRoot()` after frame loads. |
| `IFilePickerProvider` | `FilePickerProvider` | `FileOpenPicker` with HWND initialization |
| `ICameraProvider` | `CameraProvider` | `CameraCaptureUI` with HWND initialization |
| `IWindowHandleProvider` | `WindowHandleProvider` | Provides HWND via `WindowNative.GetWindowHandle()` |

### Associated Records

- `PickedFile(string FileName, string Path, long SizeBytes)` — in `Toolkit.Platform` alongside `IFilePickerProvider`
- `CapturedPhoto(string TempFilePath, long SizeBytes)` — in `Toolkit.Platform` alongside `ICameraProvider`

### CapturedPhoto Name Collision

`CapturedPhoto` collides with `Windows.Media.Capture.CapturedPhoto`. CameraProvider uses a using alias:
```csharp
using CapturedPhoto = demos_applications_winui.Toolkit.Platform.CapturedPhoto;
```

### HWND Pattern

`FilePickerProvider` and `CameraProvider` require `IWindowHandleProvider` for `InitializeWithWindow.Initialize()`. `WindowHandleProvider` is registered separately in App because it needs a `MainWindow` reference:

```csharp
services.AddSingleton<IWindowHandleProvider>(sp =>
    new WindowHandleProvider(sp.GetRequiredService<MainWindow>()));
```

## DI Registration

All registration via extension methods in `Toolkit.Configuration.ToolkitServiceCollectionExtensions`:

```csharp
services.AddNavigation();   // NavigationState, INavigationState, INavigationService
services.AddToast();        // ToastState, IToastState, IToastProvider, ToastPresenter
services.AddProviders();    // IDialogProvider, IFilePickerProvider, ICameraProvider
```

`IWindowHandleProvider` is registered in `App.xaml.cs` (not via `AddProviders()`) because it depends on `MainWindow`.
