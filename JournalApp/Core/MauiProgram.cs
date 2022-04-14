using Microsoft.EntityFrameworkCore;

namespace JournalApp;

public static class MauiProgram
{
    private static string DbFilename { get; } = Path.Combine(FileSystem.AppDataDirectory, $"journal.db");

    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder()
            .UseMauiApp<App>()
            .ConfigureFonts(fonts => fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular"));

        builder.Services.AddMauiBlazorWebView();
        builder.Services.AddAntDesign();

        builder.Services.AddDbContextFactory<ApplicationDbContext>(options => options
            .UseSqlite($"Data Source = {DbFilename}")
            .EnableSensitiveDataLogging());

        return builder.Build();
    }
}
