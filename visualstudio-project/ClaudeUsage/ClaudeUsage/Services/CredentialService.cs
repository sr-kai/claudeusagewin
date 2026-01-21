using System.IO;
using System.Text.Json;
using ClaudeUsage.Models;

namespace ClaudeUsage.Services;

public class CredentialService
{
    private static readonly string CredentialsPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        ".claude",
        ".credentials.json"
    );

    public static string? GetAccessToken()
    {
        try
        {
            if (!File.Exists(CredentialsPath))
            {
                return null;
            }

            var json = File.ReadAllText(CredentialsPath);
            var credentials = JsonSerializer.Deserialize<CredentialsFile>(json);

            return credentials?.ClaudeAiOauth?.AccessToken;
        }
        catch (Exception)
        {
            return null;
        }
    }

    public static bool CredentialsExist()
    {
        return File.Exists(CredentialsPath);
    }

    public static string GetCredentialsPath()
    {
        return CredentialsPath;
    }
}
