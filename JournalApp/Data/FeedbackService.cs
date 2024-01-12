namespace JournalApp;

public static class FeedbackService
{
    public static readonly string Email = "DCAppHelp@outlook.com";
    public static readonly string NewGitHubIssue = "https://github.com/danielchalmers/JournalApp/issues/new";

    public static string GenerateLink(string subject = null, string body = null)
    {
        var deviceInfo = DeviceInfo.Current;

        var finalSubject = "JournalApp: ";

        if (!string.IsNullOrWhiteSpace(subject))
            finalSubject += subject;

        var finalBody = $"{deviceInfo.Platform} {deviceInfo.VersionString}%0D%0A{deviceInfo.Manufacturer} {deviceInfo.Model}%0D%0A%0D%0A";

        if (!string.IsNullOrWhiteSpace(body))
            finalBody += body;

        return $"mailto:{Email}?subject={finalSubject}&body={finalBody}".Replace(" ", "%20");
    }
}
