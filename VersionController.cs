using System.Reflection;
using System.Text.Json;

namespace TarkovRichPresence;

class VersionController
{
    public static string getVersion()
    {
        var version = Assembly.GetExecutingAssembly()
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;

        return version?.Split('+')[0] ?? "Unknown";
    }

    public static async Task<LatestRelease?> getLatestRelease()
    {
        try
        {
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("TarkovRichPresence/1.0");
            httpClient.Timeout = TimeSpan.FromSeconds(10);

            var response = await httpClient.GetStringAsync("https://api.github.com/repos/solidhorizons/tarkovrichpresence/releases/latest");
            var jsonDoc = JsonDocument.Parse(response);

            var tagName = jsonDoc.RootElement.TryGetProperty("tag_name", out var tagNameElement)
                ? tagNameElement.GetString()
                : null;
            var htmlUrl = jsonDoc.RootElement.TryGetProperty("html_url", out var htmlUrlElement)
                ? htmlUrlElement.GetString()
                : null;

            if (tagName != null && htmlUrl != null)
            {
                return new LatestRelease(tagName, htmlUrl);
            }
        }
        catch (Exception ex)
        {
            FileLogger.Log($"[VersionController] Error fetching latest version: {ex}");
        }

        return null;
    }

    public static bool IsNewer(string? latestVersion, string currentVersion)
    {
        if (string.IsNullOrEmpty(latestVersion))
            return false;

        if (!TryParse(latestVersion, out var latest) || !TryParse(currentVersion, out var current))
            return false;

        return latest > current;
    }

    private static bool TryParse(string raw, out Version version)
    {
        var trimmed = raw.TrimStart('v', 'V').Split('-')[0].Split('+')[0];
        return Version.TryParse(trimmed, out version!);
    }
}

record LatestRelease(string TagName, string HtmlUrl);