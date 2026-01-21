# Tasks

## 1. Project Setup
- [x] 1.1 Create .NET 8 WPF project in Visual Studio
- [x] 1.2 Install WPF-UI NuGet package (`Wpf.Ui`)
- [x] 1.3 Configure App.xaml with Fluent theme resources
- [x] 1.4 Configure for single-file executable output
- [x] 1.5 Set up project to start minimized to tray (no main window on startup)

## 2. Credential Reading
- [x] 2.1 Implement `CredentialService.cs` to read `.credentials.json`
- [x] 2.2 Parse `claudeAiOauth.accessToken` from JSON
- [x] 2.3 Handle missing/invalid credential file gracefully

## 3. API Integration
- [x] 3.1 Implement `UsageApiService.cs` with HttpClient
- [x] 3.2 Call `https://api.anthropic.com/api/oauth/usage` with proper headers
- [x] 3.3 Parse response into `UsageData` model (five_hour, seven_day)
- [x] 3.4 Handle API errors and network failures

## 4. Tray Application
- [x] 4.1 Add NotifyIcon in App.xaml.cs (WPF doesn't have built-in tray, use System.Windows.Forms.NotifyIcon)
- [x] 4.2 Create color-coded icons programmatically (green/yellow/red/gray)
- [x] 4.3 Update tooltip with current usage percentages
- [x] 4.4 Build right-click context menu (Refresh, Launch at Login, Exit)
- [x] 4.5 Show MainWindow popup on left-click

## 5. Popup Window (MainWindow.xaml)
- [x] 5.1 Design MainWindow as borderless Fluent popup (Mica background)
- [x] 5.2 Position window near tray icon on show
- [x] 5.3 Implement header with app icon, "Claude Usage" title, version
- [x] 5.4 Create Session card using WPF-UI CardControl (label, %, ProgressBar, countdown)
- [x] 5.5 Create Weekly card using WPF-UI CardControl (label, %, ProgressBar, countdown)
- [x] 5.6 Color-code progress bars (green <70%, yellow 70-90%, red >90%)
- [x] 5.7 Add "Check for Updates" button (WPF-UI Button)
- [x] 5.8 Add "Launch at Login" toggle (WPF-UI ToggleSwitch)
- [x] 5.9 Add footer with "Updated X ago" timestamp
- [x] 5.10 Add footer icon buttons (refresh, GitHub, close) using WPF-UI SymbolIcon
- [x] 5.11 Dismiss on click-outside (Deactivated event) or Escape key
- [x] 5.12 Follow system dark/light theme automatically

## 6. Auto-Refresh
- [x] 6.1 Set up Timer for 2-minute polling interval
- [x] 6.2 Update icon/tooltip/popup on each refresh
- [x] 6.3 Handle refresh failures without crashing

## 7. Build & Package
- [ ] 7.1 Configure Release build for single-file .exe
- [ ] 7.2 Test on Windows
- [ ] 7.3 Document installation/usage in README
