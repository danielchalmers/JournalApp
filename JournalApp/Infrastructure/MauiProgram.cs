using MudBlazor.Services;

namespace JournalApp;

public static class MauiProgram
{
    private static string DbFilename { get; } = Path.Combine(FileSystem.AppDataDirectory, $"journal.db");

    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder()
            .UseMauiApp<App>()
            .ConfigureFonts(fonts => fonts.AddFont("Roboto-Regular.ttf", "RobotoRegular"));

        builder.Services.AddMauiBlazorWebView();
#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
#endif

        builder.Services.AddLogging(c =>
        {
            c.SetMinimumLevel(LogLevel.Information);
            c.AddDebug();
            c.AddConsole();
        });

        builder.Services.AddSingleton<KeycodeService>();
        builder.Services.AddSingleton<ApplicationDbSeeder>();

        builder.Services.AddMudServices(c =>
        {
            c.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomCenter;
            c.SnackbarConfiguration.NewestOnTop = true;
            c.SnackbarConfiguration.HideTransitionDuration = 300;
            c.SnackbarConfiguration.ShowTransitionDuration = 300;
            c.SnackbarConfiguration.SnackbarVariant = Variant.Text;
        });

        builder.Services.AddDbContext<ApplicationDbContext>(options => options
            .UseLazyLoadingProxies()
            .UseSqlite($"Data Source = {DbFilename}")
            .EnableSensitiveDataLogging()
            .EnableDetailedErrors());

        return builder.Build();
    }
}
