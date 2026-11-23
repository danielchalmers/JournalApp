using Microsoft.EntityFrameworkCore.Design;

namespace JournalApp;

/// <summary>
/// Creates the context for EF Core tooling, such as adding a migration or regenerating the compiled model.
/// </summary>
public class AppDbContextDesignTimeFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        // The compiled model is only valid for the options it was generated with, so these have to match the ones the app uses.
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseLazyLoadingProxies()
            .UseSqlite("Data Source = temp")
            .Options;

        return new AppDbContext(options);
    }
}
