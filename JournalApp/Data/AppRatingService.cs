using Plugin.Maui.AppRating;

namespace JournalApp;

public class AppRatingService(ILogger<AppRatingService> logger, IAppRating appRating)
{
    public async Task TryShowInAppRatingDialog()
    {
        if (!OperatingSystem.IsAndroid())
        {
            logger.LogDebug("Not on Android");
            return;
        }

        if (App.LaunchCount < 20 || LastRatingDate + TimeSpan.FromDays(90) > DateTimeOffset.Now)
        {
            logger.LogDebug("Too soon");
            return;
        }

        logger.LogInformation("Showing in-app rating prompt");
        LastRatingDate = DateTimeOffset.Now;

        // Doesn't do anything unless installed from the Play Store.
        await appRating.PerformInAppRateAsync();
    }

    private DateTimeOffset LastRatingDate
    {
        get
        {
            var lastExportString = Preferences.Get("last_rating", null);

            if (DateTimeOffset.TryParse(lastExportString, out var parsed))
            {
                return parsed;
            }
            else
            {
                LastRatingDate = default;
                return default;
            }
        }
        set => Preferences.Set("last_rating", value.ToString("O"));
    }
}
