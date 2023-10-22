using CommunityToolkit.Maui;
using MudBlazor;
using MudBlazor.Services;

namespace JournalApp;

public static class MauiProgram
{
    private static string DbFilename { get; } = Path.Combine(FileSystem.AppDataDirectory, $"journal.db");

    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder()
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts => fonts.AddFont("Roboto-Regular.ttf", "RobotoRegular"));

        builder.Services.AddMauiBlazorWebView();
        builder.Logging.AddConsole();
#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

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

        builder.Services.AddSingleton<AppDataService>();
        builder.Services.AddSingleton<ApplicationDbSeeder>();
        builder.Services.AddSingleton<KeycodeService>();

        return builder.Build();
    }
}
