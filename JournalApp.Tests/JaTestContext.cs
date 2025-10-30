using JournalApp.Data;
using MaterialColorUtilities.Maui;
using MaterialColorUtilities.Palettes;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace JournalApp.Tests;

public abstract class JaTestContext : TestContext, IAsyncLifetime
{
    private SqliteConnection _dbConnection;

    public virtual Task InitializeAsync()
    {
        Services.AddLogging();
        
        // Register MaterialColorService dependencies for ThemeService
        Services.AddSingleton<IOptions<MaterialColorOptions>>(sp =>
            Options.Create(new MaterialColorOptions { FallbackSeed = ThemeConstants.DefaultSeedColor }));
        Services.AddSingleton<IDynamicColorService, DynamicColorService>();
        Services.AddSingleton<MaterialColorService>();
        
        Services.AddCommonJournalAppServices();
        Services.AddSingleton<AppDataUIService>();
        Services.AddSingleton<IPreferences, InMemoryPreferences>();
        Services.AddSingleton<IShare, MockShare>();
        Services.AddSingleton<IBrowser, MockBrowser>();
        JSInterop.Mode = JSRuntimeMode.Loose;
        return Task.CompletedTask;
    }

    public virtual async Task DisposeAsync()
    {
        if (_dbConnection != null)
        {
            await _dbConnection.CloseAsync();
            await _dbConnection.DisposeAsync();
        }
    }

    public void AddDbContext()
    {
        if (_dbConnection == null)
        {
            _dbConnection = new SqliteConnection("Filename=:memory:");
            _dbConnection.Open();
        }

        Services.AddDbContextFactory<AppDbContext>(options => options
            .UseLazyLoadingProxies()
            .UseSqlite(_dbConnection)
        );

        var dbf = Services.GetService<IDbContextFactory<AppDbContext>>();
        var dbSeeder = Services.GetService<AppDbSeeder>();
        dbSeeder.PrepareDatabase();
    }
}
