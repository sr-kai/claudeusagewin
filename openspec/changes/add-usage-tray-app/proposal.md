# Change: Add Windows System Tray Usage Monitor

## Why
Claude Code users on Windows have no easy way to monitor their usage limits. The existing [claudecodeusage](https://github.com/richhickson/claudecodeusage) app only supports macOS. A Windows system tray app would provide at-a-glance visibility into session and weekly usage without interrupting workflow.

## What Changes
- Create a new C#/.NET 8 Windows Forms application
- Read OAuth credentials from `%USERPROFILE%\.claude\.credentials.json`
- Poll the Anthropic usage API every 2 minutes
- Display usage status via color-coded system tray icon
- Show detailed stats in context menu and tooltip

## Impact
- Affected specs: `tray-app` (new capability)
- Affected code: New project (no existing code)
- Dependencies: .NET 8 SDK, Visual Studio
