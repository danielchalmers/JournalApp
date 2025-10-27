using CommunityToolkit.Maui;
using JournalApp.Data;
using Maui.Biometric;

namespace JournalApp;

public static class MauiProgram
{
    /// <summary>
    /// The local SQLite database file path.
    /// </summary>
    public static string DbFilename { get; } = Path.Combine(FileSystem.AppDataDirectory, "journal.db");

    public static MauiApp CreateMauiApp()
    {
        var stopwatch = Stopwatch.StartNew();

        var builder = MauiApp.CreateBuilder()
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .UseBiometricAuthentication()
            .ConfigureFonts(fonts => fonts.AddFont("Roboto-Regular.ttf", "RobotoRegular"));

        // Enable Blazor WebView.
        builder.Services.AddMauiBlazorWebView();

#if DEBUG
        // Add debugging tools and log output for development.
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#else
        // Production logging to console only.
        builder.Logging.AddConsole();
#endif

        builder.Logging.SetMinimumLevel(LogLevel.Information);

        // Fine-tune logging.
        builder.Logging.AddFilter(ThisAssembly.AssemblyTitle, LogLevel.Debug);
        builder.Logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Warning); // Reduces EF noise during seed/import which increases speed.

        // Register database context with SQLite and EF Core options.
        builder.Services.AddDbContextFactory<AppDbContext>(options => options
            .UseLazyLoadingProxies()
            .UseSqlite($"Data Source = {DbFilename}")
#if DEBUG
            .EnableSensitiveDataLogging()  // Useful during development.
            .EnableDetailedErrors()
#endif
        );

        // Register core services and helpers for the app.
        builder.Services.AddCommonJournalAppServices();
        builder.Services.AddSingleton<AppDataUIService>();
        builder.Services.AddSingleton(Preferences.Default);
        builder.Services.AddSingleton(Share.Default);
        builder.Services.AddSingleton(FilePicker.Default);
        builder.Services.AddSingleton(Browser.Default);
        builder.Services.AddSingleton(Clipboard.Default);

        // Note: Database seeding moved to MainActivity to ensure it happens after authentication
        
        stopwatch.Stop();
        var logger = builder.Services.BuildServiceProvider().GetRequiredService<ILoggerFactory>().CreateLogger("JournalApp.MauiProgram");
        logger.LogInformation("Created MAUI app in {ElapsedMilliseconds}ms", stopwatch.ElapsedMilliseconds);

        return builder.Build();
    }
}
