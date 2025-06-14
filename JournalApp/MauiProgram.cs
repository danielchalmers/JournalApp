﻿using CommunityToolkit.Maui;
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
        var stopwatch = Stopwatch.StartNew();

        var builder = MauiApp.CreateBuilder()
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
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

        // Seed the database with required data.
        using var provider = builder.Services.BuildServiceProvider();
        var dbSeeder = provider.GetService<AppDbSeeder>();

        dbSeeder.PrepareDatabase();
        dbSeeder.SeedCategories();

#if DEBUG
        // Only seed sample days in debug mode.
        dbSeeder.SeedDays();
#endif

        var logger = provider.GetRequiredService<ILoggerFactory>().CreateLogger("JournalApp.MauiProgram");
        logger.LogInformation($"Created MAUI app in: {stopwatch.ElapsedMilliseconds}ms");

        return builder.Build();
    }
}
