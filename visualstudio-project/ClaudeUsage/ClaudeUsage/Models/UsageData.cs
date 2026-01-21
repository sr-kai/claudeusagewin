using System.Text.Json.Serialization;

namespace ClaudeUsage.Models;

public class UsageData
{
    [JsonPropertyName("five_hour")]
    public UsageWindow? FiveHour { get; set; }

    [JsonPropertyName("seven_day")]
    public UsageWindow? SevenDay { get; set; }

    [JsonPropertyName("sonnet_only")]
    public UsageWindow? SonnetOnly { get; set; }
}

public class UsageWindow
{
    [JsonPropertyName("utilization")]
    public double Utilization { get; set; }

    [JsonPropertyName("resets_at")]
    public DateTime ResetsAt { get; set; }

    public int UtilizationPercent => (int)(Utilization * 100);

    public string TimeUntilReset
    {
        get
        {
            var remaining = ResetsAt - DateTime.UtcNow;
            if (remaining.TotalSeconds <= 0)
                return "Resetting...";

            if (remaining.TotalDays >= 1)
                return $"{(int)remaining.TotalDays}d {remaining.Hours}h";

            if (remaining.TotalHours >= 1)
                return $"{(int)remaining.TotalHours}h {remaining.Minutes}m";

            return $"{remaining.Minutes}m";
        }
    }
}

public class CredentialsFile
{
    [JsonPropertyName("claudeAiOauth")]
    public ClaudeOAuth? ClaudeAiOauth { get; set; }
}

public class ClaudeOAuth
{
    [JsonPropertyName("accessToken")]
    public string? AccessToken { get; set; }

    [JsonPropertyName("refreshToken")]
    public string? RefreshToken { get; set; }

    [JsonPropertyName("expiresAt")]
    public long? ExpiresAt { get; set; }
}
