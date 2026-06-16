using CommunityToolkit.Maui;
using JournalApp.Data;

namespace JournalApp;

public static class MauiProgram
{
    /// <summary>
    /// The local SQLite database file path.
    /// </summary>
    public static string DbFilename { get; } = Path.Combine(FileSystem.AppDataDirectory, "journal.db");

    public static MauiApp CreateMauiApp()
    {
        var startupStopwatch = Stopwatch.StartNew();

#pragma warning disable CA1416 // MAUI startup is only used from platform entry points.
        var builder = MauiApp.CreateBuilder()
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts => fonts.AddFont("Roboto-Regular.ttf", "RobotoRegular"));
#pragma warning restore CA1416

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
        builder.Services.AddSingleton(CommunityToolkit.Maui.Storage.FileSaver.Default);

        // Seed the database with required data.
        var phaseStopwatch = Stopwatch.StartNew();
        using var provider = builder.Services.BuildServiceProvider();
        var logger = provider.GetRequiredService<ILoggerFactory>().CreateLogger("JournalApp.Startup");
        logger.LogInformation(
            "Startup phase {PhaseName} completed in {ElapsedMilliseconds}ms (total: {TotalElapsedMilliseconds}ms)",
            "BuildServiceProvider",
            phaseStopwatch.ElapsedMilliseconds,
            startupStopwatch.ElapsedMilliseconds);

        var dbSeeder = provider.GetRequiredService<AppDbSeeder>();

        phaseStopwatch.Restart();
        dbSeeder.PrepareDatabase();
        logger.LogInformation(
            "Startup phase {PhaseName} completed in {ElapsedMilliseconds}ms (total: {TotalElapsedMilliseconds}ms)",
            nameof(AppDbSeeder.PrepareDatabase),
            phaseStopwatch.ElapsedMilliseconds,
            startupStopwatch.ElapsedMilliseconds);

        phaseStopwatch.Restart();
        dbSeeder.SeedCategories();
        logger.LogInformation(
            "Startup phase {PhaseName} completed in {ElapsedMilliseconds}ms (total: {TotalElapsedMilliseconds}ms)",
            nameof(AppDbSeeder.SeedCategories),
            phaseStopwatch.ElapsedMilliseconds,
            startupStopwatch.ElapsedMilliseconds);

#if DEBUG
        // Only seed sample days in debug mode.
        phaseStopwatch.Restart();
        dbSeeder.SeedDays();
        logger.LogInformation(
            "Startup phase {PhaseName} completed in {ElapsedMilliseconds}ms (total: {TotalElapsedMilliseconds}ms)",
            "SeedDebugDays",
            phaseStopwatch.ElapsedMilliseconds,
            startupStopwatch.ElapsedMilliseconds);
#endif

        phaseStopwatch.Restart();
        var app = builder.Build();
        logger.LogInformation(
            "Startup phase {PhaseName} completed in {ElapsedMilliseconds}ms (total: {TotalElapsedMilliseconds}ms)",
            "BuildMauiApp",
            phaseStopwatch.ElapsedMilliseconds,
            startupStopwatch.ElapsedMilliseconds);

        startupStopwatch.Stop();
        logger.LogInformation("Created MAUI app in {ElapsedMilliseconds}ms", startupStopwatch.ElapsedMilliseconds);

        return app;
    }
}
