# Project Context

## Purpose
A Windows system tray app that displays Claude Code usage limits in real-time. Windows port of [claudecodeusage](https://github.com/richhickson/claudecodeusage).

## Tech Stack
- **Language**: C# (.NET 8)
- **UI**: WPF with WPF-UI library (Fluent Design)
- **IDE**: Visual Studio
- **Output**: Single-file executable
- **UI Library**: [WPF-UI](https://github.com/lepoco/wpfui) for Windows 11 Fluent styling

## Project Conventions

### Code Style
- Standard C# conventions (PascalCase for public, camelCase for private)
- Nullable reference types enabled
- Keep it simple - this is a small utility app

### Architecture
Simple single-project WPF structure:
- `App.xaml` / `App.xaml.cs` - Application entry, tray icon setup
- `MainWindow.xaml` / `MainWindow.xaml.cs` - Popup window with usage cards
- `Services/UsageApiService.cs` - HTTP calls to Anthropic API
- `Services/CredentialService.cs` - Read Claude Code OAuth token
- `Models/UsageData.cs` - Data models for API response
- `Helpers/StartupHelper.cs` - Windows startup registry management

## Domain Context

### Claude Code Usage API
- **Endpoint**: `https://api.anthropic.com/api/oauth/usage`
- **Method**: GET
- **Headers**:
  ```
  Accept: application/json
  Content-Type: application/json
  User-Agent: ClaudeUsageWindows/1.0
  Authorization: Bearer {accessToken}
  anthropic-beta: oauth-2025-04-20
  ```
- **Response**:
  ```json
  {
    "five_hour": { "utilization": 0.45, "resets_at": "2025-01-20T15:00:00Z" },
    "seven_day": { "utilization": 0.23, "resets_at": "2025-01-25T00:00:00Z" },
    "sonnet_only": { "utilization": 0.10, "resets_at": "..." }  // optional
  }
  ```
- **Note**: Undocumented API, may change without notice

### Windows Credential Storage
Claude Code stores credentials at: `%USERPROFILE%\.claude\.credentials.json`

**File format**:
```json
{
  "claudeAiOauth": {
    "accessToken": "sk-ant-oat01-...",
    "refreshToken": "sk-ant-ort01-...",
    "expiresAt": 1234567890,
    "scopes": ["user:inference", "user:profile"]
  }
}
```

Read the `claudeAiOauth.accessToken` field for API authorization.

### Usage Display
- Tray icon tooltip shows current usage %
- Color-coded icon: 🟢 <70%, 🟡 70-90%, 🔴 >90%
- Context menu shows detailed stats and reset countdown
- Auto-refresh every 2 minutes
- `five_hour` = session limit, `seven_day` = weekly limit

## Important Constraints
- Undocumented API may break without notice
- Requires Claude Code CLI installed and authenticated
- No admin privileges required

## External Dependencies
- **Anthropic Usage API**: `https://api.anthropic.com/api/oauth/usage`
- **Credentials file**: `%USERPROFILE%\.claude\.credentials.json`
