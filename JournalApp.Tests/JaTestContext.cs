using JournalApp.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace JournalApp.Tests;

public abstract class JaTestContext : TestContext, IAsyncLifetime
{
    private SqliteConnection _dbConnection;

    public virtual Task InitializeAsync()
    {
        Services.AddLogging();
        Services.AddCommonJournalAppServices();
        Services.AddSingleton<IPreferences, InMemoryPreferences>();
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
        dbSeeder.SeedDb();
        dbSeeder.SeedCategories();
    }
}
