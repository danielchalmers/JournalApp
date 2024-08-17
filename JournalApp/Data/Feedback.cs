namespace JournalApp;

public static class Feedback
{
    public static readonly string Email = "DCAppHelp@outlook.com";
    public static readonly string NewGitHubIssue = "https://github.com/danielchalmers/JournalApp/issues/new";

    public static string GenerateLink(string subject = null, string body = null)
    {
        var deviceInfo = DeviceInfo.Current;
        var deviceInfoString = $"{ThisAssembly.AssemblyInformationalVersion} | {deviceInfo.Platform} {deviceInfo.VersionString} | {deviceInfo.Manufacturer} {deviceInfo.Model}";

        subject = $"{ThisAssembly.AssemblyTitle}: " + subject;

        body += "%0D%0A" + deviceInfoString + "%0D%0A";

        return $"mailto:{Email}?subject={subject}&body=%0D%0A{body}".Replace(" ", "%20");
    }
}
