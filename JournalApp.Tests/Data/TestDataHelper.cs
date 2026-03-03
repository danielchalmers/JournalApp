using Microsoft.EntityFrameworkCore;

namespace JournalApp.Tests.Data;

public static class TestDataHelper
{
    public static (IDbContextFactory<AppDbContext> DbFactory, AppDbSeeder Seeder, AppDataService DataService) ResolveDataServices(
        IServiceProvider services)
    {
        return (
            services.GetService<IDbContextFactory<AppDbContext>>(),
            services.GetService<AppDbSeeder>(),
            services.GetService<AppDataService>());
    }

    public static void SeedCategoriesAndDays(AppDbSeeder seeder, DateOnly startDate, DateOnly endDate)
    {
        seeder.SeedCategories();
        seeder.SeedDays(startDate.DatesTo(endDate));
    }

    public static int ExpectedVisibleGroupCount(AppDbContext db, bool hideNotes)
    {
        var groups = db.Categories
            .Where(c => c.Enabled && !c.Deleted)
            .Select(c => c.Group)
            .Distinct()
            .ToList();

        if (hideNotes)
        {
            groups.Remove("Notes");
        }

        return groups.Count;
    }
}
