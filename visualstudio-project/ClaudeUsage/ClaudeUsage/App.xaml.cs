using System.Windows.Controls;
using System.Windows.Threading;
using ClaudeUsage.Helpers;
using ClaudeUsage.Models;
using ClaudeUsage.Services;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;
using Svg;
using Forms = System.Windows.Forms;
using Drawing = System.Drawing;

namespace ClaudeUsage;

public partial class App : System.Windows.Application
{
    private Forms.NotifyIcon? _notifyIcon;
    private MainWindow? _mainWindow;
    private DispatcherTimer? _refreshTimer;
    private UsageData? _lastUsageData;
    private DateTime _lastUpdated;
    private ContextMenu? _contextMenu;
    private System.Windows.Controls.MenuItem? _launchAtLoginItem;
    private DateTime _lastDeactivated;

    private Drawing.Icon? _currentIcon;

    protected override async void OnStartup(System.Windows.StartupEventArgs e)
    {
        base.OnStartup(e);

        // Apply system theme and listen for changes
        ApplicationThemeManager.ApplySystemTheme();
        ApplicationThemeManager.Changed += OnThemeChanged;

        // Create the tray icon
        CreateTrayIcon();

        // Create the main window (hidden initially)
        _mainWindow = new MainWindow();
        _mainWindow.Deactivated += (s, args) =>
        {
            _lastDeactivated = DateTime.Now;
            _mainWindow.HideWithAnimation();
        };

        // Set up auto-refresh timer (2 minutes)
        _refreshTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMinutes(2)
        };
        _refreshTimer.Tick += async (s, args) => await RefreshUsageData();
        _refreshTimer.Start();

        // Initial data fetch
        await RefreshUsageData();
    }

    private Drawing.Icon CreateUsageIcon(int percentage, Drawing.Color bgColor)
    {
        // Try to load SVG icon from embedded resources
        var resourceName = GetSvgResourceName(percentage);
        var svgDoc = LoadSvgFromResource(resourceName);

        if (svgDoc != null)
        {
            return CreateIconFromSvg(svgDoc, bgColor);
        }

        // Fallback to programmatic drawing
        return CreateFallbackIcon(percentage, bgColor);
    }

    private string GetSvgResourceName(int percentage)
    {
        // Available icons: 0, 10, 20, 30, 40, 50, 60, 70, 80, 90, 95, 99, 100
        int iconValue;
        if (percentage >= 100) iconValue = 100;
        else if (percentage >= 99) iconValue = 99;
        else if (percentage >= 95) iconValue = 95;
        else if (percentage < 10) iconValue = 0; // Use 0 for 0-9% (sunglasses)
        else iconValue = (percentage / 10) * 10; // Round down to nearest 10

        return $"{iconValue}.svg";
    }

    private SvgDocument? LoadSvgFromResource(string fileName)
    {
        var assembly = System.Reflection.Assembly.GetExecutingAssembly();
        var resourceNames = assembly.GetManifestResourceNames();
        var resourceName = resourceNames.FirstOrDefault(r => r.EndsWith(fileName));

        if (resourceName == null) return null;

        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream == null) return null;

        return SvgDocument.Open<SvgDocument>(stream);
    }

    private Drawing.Icon CreateIconFromSvg(SvgDocument svgDoc, Drawing.Color dotColor)
    {

        // Detect if dark theme
        var isDarkTheme = ApplicationThemeManager.GetAppTheme() == Wpf.Ui.Appearance.ApplicationTheme.Dark;
        var frameColor = isDarkTheme ? Drawing.Color.White : Drawing.Color.FromArgb(36, 36, 36);

        // Path 0: "10" text - use frame color
        if (svgDoc.Children.Count > 0 && svgDoc.Children[0] is SvgPath textPath)
        {
            textPath.Fill = new SvgColourServer(frameColor);
        }

        // Path 1: Rectangle outline - use frame color
        if (svgDoc.Children.Count > 1 && svgDoc.Children[1] is SvgPath rectPath)
        {
            rectPath.Fill = new SvgColourServer(frameColor);
        }

        // Circle (index 2): Dot - use usage color
        if (svgDoc.Children.Count > 2 && svgDoc.Children[2] is SvgCircle dotCircle)
        {
            dotCircle.Fill = new SvgColourServer(dotColor);
        }

        // Render to bitmap
        using var bitmap = svgDoc.Draw(32, 32);
        return Drawing.Icon.FromHandle(bitmap.GetHicon());
    }

    private Drawing.Icon CreateFallbackIcon(int percentage, Drawing.Color bgColor)
    {
        const int size = 32;
        const int cornerRadius = 6;

        using var bitmap = new Drawing.Bitmap(size, size);
        using var g = Drawing.Graphics.FromImage(bitmap);

        g.SmoothingMode = Drawing.Drawing2D.SmoothingMode.HighQuality;
        g.TextRenderingHint = Drawing.Text.TextRenderingHint.ClearTypeGridFit;
        g.Clear(Drawing.Color.Transparent);

        // Draw rounded rectangle background
        using var bgBrush = new Drawing.SolidBrush(bgColor);
        using var path = new Drawing.Drawing2D.GraphicsPath();
        var rect = new Drawing.Rectangle(2, 2, size - 4, size - 4);
        path.AddArc(rect.X, rect.Y, cornerRadius, cornerRadius, 180, 90);
        path.AddArc(rect.Right - cornerRadius, rect.Y, cornerRadius, cornerRadius, 270, 90);
        path.AddArc(rect.Right - cornerRadius, rect.Bottom - cornerRadius, cornerRadius, cornerRadius, 0, 90);
        path.AddArc(rect.X, rect.Bottom - cornerRadius, cornerRadius, cornerRadius, 90, 90);
        path.CloseFigure();
        g.FillPath(bgBrush, path);

        // Draw percentage number centered
        using var textFont = new Drawing.Font("Segoe UI Semibold", 10, Drawing.FontStyle.Regular);
        using var textBrush = new Drawing.SolidBrush(Drawing.Color.White);

        var text = percentage.ToString();
        var textSize = g.MeasureString(text, textFont);
        var textX = (size - textSize.Width) / 2 + 1;
        var textY = (size - textSize.Height) / 2 + 1;
        g.DrawString(text, textFont, textBrush, textX, textY);

        return Drawing.Icon.FromHandle(bitmap.GetHicon());
    }

    private Drawing.Color GetColorForUsage(double utilization)
    {
        if (utilization >= 90) return Drawing.Color.FromArgb(239, 68, 68);     // Red
        if (utilization >= 70) return Drawing.Color.FromArgb(234, 179, 8);     // Yellow
        return Drawing.Color.FromArgb(34, 197, 94);                             // Green
    }

    private void UpdateTrayIconError()
    {
        var oldIcon = _currentIcon;
        var svgDoc = LoadSvgFromResource("error.svg");
        if (svgDoc != null)
        {
            _currentIcon = CreateIconFromSvg(svgDoc, Drawing.Color.FromArgb(156, 163, 175)); // Gray dot
            _notifyIcon!.Icon = _currentIcon;
        }
        oldIcon?.Dispose();
    }

    private void OnThemeChanged(Wpf.Ui.Appearance.ApplicationTheme currentTheme, System.Windows.Media.Color systemAccent)
    {
        // Refresh the icon with current usage data to apply new theme colors
        if (_lastUsageData != null)
        {
            var sessionUtilization = _lastUsageData.FiveHour?.Utilization ?? 0;
            var maxUtilization = Math.Max(
                sessionUtilization,
                _lastUsageData.SevenDay?.Utilization ?? 0
            );
            UpdateTrayIcon((int)sessionUtilization, maxUtilization);
        }
    }

    private void UpdateTrayIcon(int percentage, double utilization)
    {
        var oldIcon = _currentIcon;
        _currentIcon = CreateUsageIcon(percentage, GetColorForUsage(utilization));
        _notifyIcon!.Icon = _currentIcon;
        oldIcon?.Dispose();
    }

    private void CreateTrayIcon()
    {
        _currentIcon = CreateUsageIcon(0, Drawing.Color.FromArgb(156, 163, 175)); // Gray
        _notifyIcon = new Forms.NotifyIcon
        {
            Icon = _currentIcon,
            Visible = true,
            Text = "Claude Usage - Loading..."
        };

        // Create WPF context menu with Fluent styling
        CreateContextMenu();

        // Left-click shows the popup, right-click shows context menu
        _notifyIcon.MouseClick += (s, e) =>
        {
            if (e.Button == Forms.MouseButtons.Left)
            {
                ShowPopup();
            }
            else if (e.Button == Forms.MouseButtons.Right)
            {
                ShowContextMenu();
            }
        };
    }

    private void CreateContextMenu()
    {
        _contextMenu = new ContextMenu();

        var refreshItem = new System.Windows.Controls.MenuItem
        {
            Header = "Refresh Now",
            Icon = new SymbolIcon { Symbol = SymbolRegular.ArrowClockwise24 }
        };
        refreshItem.Click += async (s, e) => await RefreshUsageData();

        _launchAtLoginItem = new System.Windows.Controls.MenuItem
        {
            Header = "Launch at Login",
            IsCheckable = true,
            IsChecked = StartupHelper.IsLaunchAtLoginEnabled()
        };
        _launchAtLoginItem.Click += (s, e) =>
        {
            StartupHelper.SetLaunchAtLogin(_launchAtLoginItem.IsChecked);
        };

        var exitItem = new System.Windows.Controls.MenuItem
        {
            Header = "Exit",
            Icon = new SymbolIcon { Symbol = SymbolRegular.Dismiss24 }
        };
        exitItem.Click += (s, e) =>
        {
            _notifyIcon!.Visible = false;
            Shutdown();
        };

        _contextMenu.Items.Add(refreshItem);
        _contextMenu.Items.Add(_launchAtLoginItem);
        _contextMenu.Items.Add(new Separator());
        _contextMenu.Items.Add(exitItem);
    }

    private void ShowContextMenu()
    {
        if (_contextMenu == null) return;

        // Update launch at login state
        if (_launchAtLoginItem != null)
        {
            _launchAtLoginItem.IsChecked = StartupHelper.IsLaunchAtLoginEnabled();
        }

        _contextMenu.IsOpen = true;
    }

    private void ShowPopup()
    {
        if (_mainWindow == null) return;

        // If window was just closed by clicking tray icon, don't reopen it
        // (the click causes Deactivated which hides it, then this runs)
        if ((DateTime.Now - _lastDeactivated).TotalMilliseconds < 500)
        {
            return;
        }

        // Update the window with latest data
        _mainWindow.UpdateUsageData(_lastUsageData, _lastUpdated);

        // Position near the tray icon (bottom-right of screen)
        var workArea = System.Windows.SystemParameters.WorkArea;
        var targetLeft = workArea.Right - _mainWindow.Width - 10;
        var targetTop = workArea.Bottom - _mainWindow.Height - 10;

        _mainWindow.ShowWithAnimation(targetLeft, targetTop);
    }

    public async Task RefreshUsageData()
    {
        if (!CredentialService.CredentialsExist())
        {
            UpdateTrayIconError();
            _notifyIcon!.Text = "Claude Usage - No credentials found\nRun 'claude' to authenticate";
            return;
        }

        var usage = await UsageApiService.GetUsageAsync();

        if (usage == null)
        {
            UpdateTrayIconError();
            _notifyIcon!.Text = "Claude Usage - Failed to fetch data";
            return;
        }

        _lastUsageData = usage;
        _lastUpdated = DateTime.Now;

        // Update icon based on usage (utilization is already a percentage, e.g. 8.0 = 8%)
        var sessionUtilization = usage.FiveHour?.Utilization ?? 0;
        var maxUtilization = Math.Max(
            sessionUtilization,
            usage.SevenDay?.Utilization ?? 0
        );

        UpdateTrayIcon((int)sessionUtilization, maxUtilization);

        // Update tooltip
        var sessionPct = usage.FiveHour?.UtilizationPercent ?? 0;
        var weeklyPct = usage.SevenDay?.UtilizationPercent ?? 0;
        var sessionReset = usage.FiveHour?.TimeUntilReset ?? "N/A";
        var weeklyReset = usage.SevenDay?.TimeUntilReset ?? "N/A";

        _notifyIcon.Text = $"Claude Usage\nSession: {sessionPct}% (resets in {sessionReset})\nWeekly: {weeklyPct}% (resets in {weeklyReset})";

        // Update popup if visible
        if (_mainWindow?.IsVisible == true)
        {
            _mainWindow.UpdateUsageData(_lastUsageData, _lastUpdated);
        }
    }

    protected override void OnExit(System.Windows.ExitEventArgs e)
    {
        ApplicationThemeManager.Changed -= OnThemeChanged;
        _notifyIcon?.Dispose();
        _currentIcon?.Dispose();
        base.OnExit(e);
    }
}
