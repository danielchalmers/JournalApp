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

        builder.Services.AddSingleton<KeycodeService>();

        builder.Services.AddMudServices(c =>
        {
            c.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomCenter;
            c.SnackbarConfiguration.NewestOnTop = true;
            c.SnackbarConfiguration.HideTransitionDuration = 300;
            c.SnackbarConfiguration.ShowTransitionDuration = 300;
            c.SnackbarConfiguration.SnackbarVariant = Variant.Text;
        });

        builder.Services.AddDbContextFactory<ApplicationDbContext>(options => options
            .UseLazyLoadingProxies()
            .UseSqlite($"Data Source = {DbFilename}")
            .LogTo(message => Debug.WriteLine(message), LogLevel.Information)
            .EnableSensitiveDataLogging());

        // Seed database.
        using (var serviceProvider = builder.Services.BuildServiceProvider())
        using (var dbContext = serviceProvider.GetRequiredService<ApplicationDbContext>())
            ApplicationDbSeedData.SeedAsync(dbContext).GetAwaiter().GetResult();

        return builder.Build();
    }
}
