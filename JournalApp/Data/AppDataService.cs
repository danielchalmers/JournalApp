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
        var pointsDeleted = await db.Points.ExecuteDeleteAsync();
        var daysDeleted = await db.Days.ExecuteDeleteAsync();
        var categoriesDeleted = await db.Categories.ExecuteDeleteAsync();
        await db.SaveChangesAsync();

        sw.Stop();
        logger.LogInformation(
            "Cleared data sets in {ElapsedMilliseconds}ms (points: {PointsDeleted}, days: {DaysDeleted}, categories: {CategoriesDeleted})",
            sw.ElapsedMilliseconds,
            pointsDeleted,
            daysDeleted,
            categoriesDeleted);
    }

    public async Task RestoreDbSets(BackupFile backup)
    {
        var sw = Stopwatch.StartNew();

        await using var db = await dbFactory.CreateDbContextAsync();
        await db.Days.AddRangeAsync(backup.Days);
        await db.Categories.AddRangeAsync(backup.Categories);
        await db.Points.AddRangeAsync(backup.Points);
        await db.SaveChangesAsync();

        sw.Stop();
        logger.LogInformation(
            "Restored data sets in {ElapsedMilliseconds}ms (days: {DayCount}, categories: {CategoryCount}, points: {PointCount})",
            sw.ElapsedMilliseconds,
            backup.Days.Count,
            backup.Categories.Count,
            backup.Points.Count);
    }

    public void SetPreferences(BackupFile backup)
    {
        logger.LogDebug("Restoring {PreferenceCount} preferences from backup", backup.PreferenceBackups.Count);
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
        var sw = Stopwatch.StartNew();
        await using var db = await dbFactory.CreateDbContextAsync();

        var days = await db.Days.Include(d => d.Points).ToListAsync();
        var categories = await db.Categories.Include(c => c.Points).ToListAsync();
        var points = await db.Points.ToListAsync();
        var preferencesBackup = GetPreferenceBackups().ToList();

        sw.Stop();
        logger.LogInformation(
            "Created backup snapshot in {ElapsedMilliseconds}ms (days: {DayCount}, categories: {CategoryCount}, points: {PointCount}, preferences: {PreferenceCount})",
            sw.ElapsedMilliseconds,
            days.Count,
            categories.Count,
            points.Count,
            preferencesBackup.Count);

        return new()
        {
            Days = days,
            Categories = categories,
            Points = points,
            PreferenceBackups = preferencesBackup,
        };
    }
}
