# WinUI Platform Gotchas

Issues and workarounds specific to WinUI 3 / WindowsAppSDK.

## XAML and Controls

- **`TitleBar.Footer` does not exist** in WindowsAppSDK 1.8 — use `TitleBar.Content` instead
- **No implicit usings** — all `System.*` usings must be explicit in every file
- **`PasswordBox.Password`** doesn't support `x:Bind TwoWay` reliably — use `PasswordChanged` event to push value to VM
- **UserControl with DI constructor parameters** — WinUI XAML instantiation requires parameterless constructors. Resolve via DI and add to the visual tree programmatically (e.g., `RootGrid.Children.Add(control)`)

## Pickers and Capture

- **`FileOpenPicker`** requires HWND initialization: `InitializeWithWindow.Initialize(picker, hwnd)`
- **`CameraCaptureUI`** requires HWND initialization: `InitializeWithWindow.Initialize(capture, hwnd)`
- Both consume `IWindowHandleProvider` which provides `WindowNative.GetWindowHandle(window)`
- **`webcam` capability** must be declared in `Package.appxmanifest` for camera capture

## WebView2

- **Singleton pages with WebView2** — guard against re-initialization in the `Loaded` handler: `if (_bridge is not null) return;`
- WebView2 content loaded via virtual host mapping: `CoreWebView2.SetVirtualHostNameToFolderMapping("app.local", assetsPath, ...)`
- C# to JS: `ExecuteScriptAsync("functionName(...)")`
- JS to C#: `window.chrome.webview.postMessage(JSON.stringify({type, data}))` via `WebMessageReceived` event

## ContentDialog

- **Requires `XamlRoot`** — `DialogProvider.SetXamlRoot()` must be called after the root frame is loaded (done in MainWindow via `rootFrame.Loaded` event)
- Only one `ContentDialog` can be open at a time — attempting to show a second throws

## Build

- **No AnyCPU** — must specify platform: `dotnet build -p:Platform=ARM64`
- `UseWinUI` and `WinUISDKReferences` properties are only needed in projects that use WinUI XAML types (Toolkit, Auth, Notes, App — not Core)
