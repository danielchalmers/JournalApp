using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace JournalApp.Tests;

public class JaTestContext : TestContext, IAsyncLifetime
{
    private SqliteConnection _dbConnection;

    //public DateOnly SeededDate { get; } = new(2024, 01, 01);

    public Task InitializeAsync()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddLogging();
        Services.AddMudServices();
        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    public void AddDbContext(bool seed = true)
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

        if (seed)
        {
            var dbf = Services.GetService<IDbContextFactory<AppDbContext>>();
            var logger = new NullLogger<AppDbSeeder>();
            var appDbSeeder = new AppDbSeeder(logger, dbf);
            appDbSeeder.SeedDb();
            appDbSeeder.SeedCategories();
            //appDbSeeder.SeedDays(SeededDate);
        }
    }
}