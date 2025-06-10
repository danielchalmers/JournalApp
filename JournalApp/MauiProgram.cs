using CommunityToolkit.Maui;
using JournalApp.Data;

namespace JournalApp;

public static class MauiProgram
{
    public static string DbFilename { get; } = Path.Combine(FileSystem.AppDataDirectory, "journal.db");

    public static MauiApp CreateMauiApp()
    {
        var stopwatch = Stopwatch.StartNew();
        var builder = MauiApp.CreateBuilder()
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts => fonts.AddFont("Roboto-Regular.ttf", "RobotoRegular"));

        builder.Services.AddMauiBlazorWebView();
#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#else
        builder.Logging.AddConsole();
#endif
        builder.Logging.SetMinimumLevel(LogLevel.Information);
        builder.Logging.AddFilter(ThisAssembly.AssemblyTitle, LogLevel.Debug);
        builder.Logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Warning); // Hugely slows down seed and import.

        builder.Services.AddDbContextFactory<AppDbContext>(options => options
            .UseLazyLoadingProxies()
            .UseSqlite($"Data Source = {DbFilename}")
#if DEBUG
            .EnableSensitiveDataLogging()
            .EnableDetailedErrors()
#endif
        );

        builder.Services.AddCommonJournalAppServices();

        builder.Services.AddSingleton<AppDataUIService>();
        builder.Services.AddSingleton(Preferences.Default);
        builder.Services.AddSingleton(Share.Default);
        builder.Services.AddSingleton(FilePicker.Default);
        builder.Services.AddSingleton(Browser.Default);
        builder.Services.AddSingleton(Clipboard.Default);

        // Seed the database.
        using var provider = builder.Services.BuildServiceProvider();
        var dbSeeder = provider.GetService<AppDbSeeder>();
        dbSeeder.PrepareDatabase();
        dbSeeder.SeedCategories();
#if DEBUG
        dbSeeder.SeedDays();
#endif

        var logger = provider.GetRequiredService<ILoggerFactory>().CreateLogger("JournalApp.MauiProgram");
        logger.LogInformation($"Created MAUI app in: {stopwatch.ElapsedMilliseconds}ms");

        return builder.Build();
    }
}
