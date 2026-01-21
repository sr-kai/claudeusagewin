## ADDED Requirements

### Requirement: Fluent Design UI
The application SHALL use Windows 11 Fluent Design styling via WPF-UI library to provide a modern, native appearance on Windows 10/11.

#### Scenario: Fluent styling applied
- **WHEN** the application runs on Windows 10 or 11
- **THEN** the popup window displays with Mica/Acrylic background, rounded corners, and Fluent typography

#### Scenario: Theme follows system
- **WHEN** the user has Windows set to dark or light mode
- **THEN** the application matches the system theme automatically

---

### Requirement: Credential Reading
The application SHALL read Claude Code OAuth credentials from the file `%USERPROFILE%\.claude\.credentials.json` and extract the `claudeAiOauth.accessToken` field for API authorization.

#### Scenario: Valid credentials file exists
- **WHEN** the credentials file exists and contains valid JSON with `claudeAiOauth.accessToken`
- **THEN** the application extracts the access token for API calls

#### Scenario: Credentials file missing
- **WHEN** the credentials file does not exist at the expected path
- **THEN** the application displays a tray notification indicating credentials are missing and instructions to authenticate with Claude Code CLI

#### Scenario: Invalid credentials format
- **WHEN** the credentials file exists but is malformed or missing required fields
- **THEN** the application displays an error notification and continues running with a disabled state

---

### Requirement: Usage API Integration
The application SHALL call the Anthropic usage API at `https://api.anthropic.com/api/oauth/usage` with proper authentication headers to retrieve current usage data.

#### Scenario: Successful API call
- **WHEN** the API returns a successful response
- **THEN** the application parses `five_hour.utilization`, `five_hour.resets_at`, `seven_day.utilization`, and `seven_day.resets_at` fields

#### Scenario: API authentication failure
- **WHEN** the API returns 401 Unauthorized
- **THEN** the application displays a notification that credentials may be expired and prompts re-authentication

#### Scenario: Network failure
- **WHEN** the API call fails due to network issues
- **THEN** the application retains the last known usage data and displays a warning icon or indicator

---

### Requirement: System Tray Display
The application SHALL display usage status in the Windows system tray with a color-coded icon based on the higher of session or weekly utilization.

#### Scenario: Low usage (green)
- **WHEN** both session and weekly utilization are below 70%
- **THEN** the tray icon displays in green

#### Scenario: Medium usage (yellow)
- **WHEN** either session or weekly utilization is between 70% and 90%
- **THEN** the tray icon displays in yellow

#### Scenario: High usage (red)
- **WHEN** either session or weekly utilization is above 90%
- **THEN** the tray icon displays in red

---

### Requirement: Tooltip Information
The application SHALL display current usage percentages in the tray icon tooltip for quick reference.

#### Scenario: Tooltip content
- **WHEN** the user hovers over the tray icon
- **THEN** a tooltip displays showing session usage %, weekly usage %, and time until next reset

---

### Requirement: Popup Window
The application SHALL display a popup window when left-clicking the tray icon, showing detailed usage information in a modern card-based UI.

#### Scenario: Popup appears on click
- **WHEN** the user left-clicks the tray icon
- **THEN** a popup window appears near the tray icon with usage details

#### Scenario: Popup content - Header
- **WHEN** the popup is displayed
- **THEN** it shows the app icon, "Claude Usage" title, and version number

#### Scenario: Popup content - Session card
- **WHEN** the popup is displayed
- **THEN** it shows a "Session" card with: "5-hour window" label, usage percentage, color-coded progress bar, and "Resets in Xh Xm" countdown

#### Scenario: Popup content - Weekly card
- **WHEN** the popup is displayed
- **THEN** it shows a "Weekly" card with: "7-day window" label, usage percentage, color-coded progress bar, and "Resets in Xd Xh" countdown

#### Scenario: Popup content - Controls
- **WHEN** the popup is displayed
- **THEN** it shows: "Check for Updates" button, "Launch at Login" checkbox, and "Updated X seconds ago" timestamp

#### Scenario: Popup content - Footer buttons
- **WHEN** the popup is displayed
- **THEN** it shows icon buttons for: refresh, GitHub link, and close/exit

#### Scenario: Popup dismissal
- **WHEN** the user clicks outside the popup or presses Escape
- **THEN** the popup closes

#### Scenario: Launch at Login toggle
- **WHEN** the user toggles "Launch at Login"
- **THEN** the application adds/removes itself from Windows startup

---

### Requirement: Context Menu
The application SHALL provide a context menu when right-clicking the tray icon with quick actions.

#### Scenario: Menu contents
- **WHEN** the user right-clicks the tray icon
- **THEN** a menu displays with: "Refresh Now", "Launch at Login" toggle, and "Exit" options

#### Scenario: Refresh action
- **WHEN** the user clicks "Refresh Now"
- **THEN** the application immediately fetches fresh usage data from the API

#### Scenario: Exit action
- **WHEN** the user clicks "Exit"
- **THEN** the application closes cleanly

---

### Requirement: Auto-Refresh
The application SHALL automatically refresh usage data at a configurable interval (default 2 minutes).

#### Scenario: Periodic refresh
- **WHEN** 2 minutes have elapsed since the last refresh
- **THEN** the application fetches fresh usage data and updates the display

#### Scenario: Refresh on startup
- **WHEN** the application starts
- **THEN** it immediately fetches usage data before displaying the tray icon
