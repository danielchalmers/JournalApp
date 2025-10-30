using MudBlazor;
using MudBlazor.Services;

namespace JournalApp.Data;

public static class CommonServices
{
    /// <summary>
    /// Adds common services used between the main app and its tests.
    /// </summary>
    public static void AddCommonJournalAppServices(this IServiceCollection services)
    {
        services.AddMudServices(c =>
        {
            c.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomCenter;
            c.SnackbarConfiguration.NewestOnTop = true;
            c.SnackbarConfiguration.HideTransitionDuration = 300;
            c.SnackbarConfiguration.ShowTransitionDuration = 300;
            c.SnackbarConfiguration.VisibleStateDuration = 10000;
            c.SnackbarConfiguration.SnackbarVariant = Variant.Filled;
        });

        services.AddSingleton<AppDataService>();
        services.AddSingleton<AppDbSeeder>();
        services.AddSingleton<KeyEventService>();
        services.AddSingleton<PreferenceService>();
        services.AddSingleton<CalendarService>();
        services.AddSingleton<BackupCreator>();
        services.AddSingleton<ExportWizard>();
    }
}
