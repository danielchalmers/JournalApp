namespace JournalApp;

/// <summary>
/// Provides services for managing app data, including backup and restore operations.
/// </summary>
public sealed class AppDataService(ILogger<AppDataService> logger, IDbContextFactory<AppDbContext> dbFactory, PreferenceService preferences)
{
    public async Task DeleteDbSets()
    {
        var sw = Stopwatch.StartNew();

        await using var db = await dbFactory.CreateDbContextAsync();
        await db.Points.ExecuteDeleteAsync();
        await db.Days.ExecuteDeleteAsync();
        await db.Categories.ExecuteDeleteAsync();
        await db.SaveChangesAsync();

        logger.LogDebug($"Cleared db sets after {sw.ElapsedMilliseconds}ms");
    }

    public async Task RestoreDbSets(BackupFile backup)
    {
        var sw = Stopwatch.StartNew();

        await using var db = await dbFactory.CreateDbContextAsync();
        await db.Days.AddRangeAsync(backup.Days);
        await db.Categories.AddRangeAsync(backup.Categories);
        await db.Points.AddRangeAsync(backup.Points);
        await db.SaveChangesAsync();

        logger.LogDebug($"Restored db sets after {sw.ElapsedMilliseconds}ms");
    }

    public void SetPreferences(BackupFile backup)
    {
        preferences.Set(backup.PreferenceBackups.ToDictionary(b => b.Name, b => b.Value));
    }

    public IEnumerable<PreferenceBackup> GetPreferenceBackups()
    {
        foreach (var item in preferences.Get(
            "safety_plan",
            "mood_palette",
            "tip_click_mood_grid_day",
            "tip_add_new_category"))
        {
            yield return new(item.Key, item.Value);
        }
    }

    public async Task<BackupFile> CreateBackup()
    {
        await using var db = await dbFactory.CreateDbContextAsync();

        return new()
        {
            Days = await db.Days.Include(d => d.Points).ToListAsync(),
            Categories = await db.Categories.Include(c => c.Points).ToListAsync(),
            Points = await db.Points.ToListAsync(),
            PreferenceBackups = GetPreferenceBackups().ToList(),
        };
    }
}
