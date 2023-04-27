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
        builder.Services.AddMudServices();
        builder.Services.AddSingleton<FocusService>();

        builder.Services.AddDbContextFactory<ApplicationDbContext>(options => options
            .UseSqlite($"Data Source = {DbFilename}")
            .EnableSensitiveDataLogging());

        return builder.Build();
    }
}
