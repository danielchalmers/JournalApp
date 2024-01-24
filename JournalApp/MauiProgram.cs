using CommunityToolkit.Maui;
using MudBlazor;
using MudBlazor.Services;

namespace JournalApp;

public static class MauiProgram
{
    public static string DbFilename { get; } = Path.Combine(FileSystem.AppDataDirectory, "journal.db");

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
        builder.Logging.SetMinimumLevel(LogLevel.Information);
        builder.Logging.AddFilter(ThisAssembly.AssemblyTitle, LogLevel.Debug);

        builder.Services.AddMudServices(c =>
        {
            c.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomCenter;
            c.SnackbarConfiguration.NewestOnTop = true;
            c.SnackbarConfiguration.HideTransitionDuration = 300;
            c.SnackbarConfiguration.ShowTransitionDuration = 300;
            c.SnackbarConfiguration.SnackbarVariant = Variant.Text;
        });

        builder.Services.AddDbContextFactory<AppDbContext>(options => options
            .UseLazyLoadingProxies()
            .UseSqlite($"Data Source = {DbFilename}")
#if DEBUG
        .EnableSensitiveDataLogging()
#endif
        );

        builder.Services.AddSingleton<AppDataService>();
        builder.Services.AddSingleton<AppDbSeeder>();
        builder.Services.AddSingleton<KeyEventService>();
        builder.Services.AddSingleton(Share.Default);
        builder.Services.AddSingleton<AppThemeService>();

        // Seed the database.
        using var provider = builder.Services.BuildServiceProvider();
        var dbSeeder = provider.GetService<AppDbSeeder>();
        dbSeeder.SeedDb();
        dbSeeder.SeedCategories();
#if DEBUG
        dbSeeder.SeedDays();
#endif

        return builder.Build();
    }
}
