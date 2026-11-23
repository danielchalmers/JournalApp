namespace JournalApp;

/// <summary>
/// Provides services for managing app data, including backup and restore operations.
/// </summary>
public sealed class AppDataService(ILogger<AppDataService> logger, IDbContextFactory<AppDbContext> dbFactory, PreferenceService preferences, AppDbSeeder dbSeeder)
{
    public async Task DeleteDbSets()
    {
        var sw = Stopwatch.StartNew();

        await using var db = await dbFactory.CreateDbContextAsync();
        await DeleteDbSetsInternal(db);
        await db.SaveChangesAsync();

        sw.Stop();
        logger.LogInformation("Cleared data sets in {ElapsedMilliseconds}ms", sw.ElapsedMilliseconds);
    }

    public async Task RestoreDbSets(BackupFile backup)
    {
        var sw = Stopwatch.StartNew();

        await using var db = await dbFactory.CreateDbContextAsync();
        await RestoreDbSetsInternal(db, backup);

        sw.Stop();
        logger.LogInformation(
            "Restored data sets in {ElapsedMilliseconds}ms (days: {DayCount}, categories: {CategoryCount}, points: {PointCount})",
            sw.ElapsedMilliseconds,
            backup.Days.Count,
            backup.Categories.Count,
            backup.Points.Count);
    }

    /// <summary>
    /// Atomically deletes all existing data and restores from backup in a single transaction.
    /// This prevents database corruption if the operation is interrupted.
    /// Built-in categories the backup doesn't have are added back afterwards, like they would be on the next launch.
    /// </summary>
    public async Task ReplaceDbSets(BackupFile backup)
    {
        var sw = Stopwatch.StartNew();

        await using var db = await dbFactory.CreateDbContextAsync();
        await using var transaction = await db.Database.BeginTransactionAsync();

        try
        {
            // Delete all existing data
            await DeleteDbSetsInternal(db);

            // Restore from backup
            await RestoreDbSetsInternal(db, backup);

            // Commit the transaction - both delete and restore succeed or fail together
            await transaction.CommitAsync();

            sw.Stop();
            logger.LogInformation(
                "Replaced data sets atomically in {ElapsedMilliseconds}ms (days: {DayCount}, categories: {CategoryCount}, points: {PointCount})",
                sw.ElapsedMilliseconds,
                backup.Days.Count,
                backup.Categories.Count,
                backup.Points.Count);
        }
        catch
        {
            // Rollback on any error - database remains in original state
            await transaction.RollbackAsync();
            throw;
        }

        try
        {
            // The app needs its built-in categories, the notes one above all, and a backup isn't guaranteed to have them.
            dbSeeder.SeedCategories();
        }
        catch (Exception ex)
        {
            // The import itself is committed, and the next launch seeds again.
            logger.LogError(ex, "Couldn't seed categories after replacing data sets");
        }
    }

    private async Task DeleteDbSetsInternal(AppDbContext db)
    {
        var pointsDeleted = await db.Points.ExecuteDeleteAsync();
        var daysDeleted = await db.Days.ExecuteDeleteAsync();
        var categoriesDeleted = await db.Categories.ExecuteDeleteAsync();

        logger.LogDebug(
            "Cleared data sets (points: {PointsDeleted}, days: {DaysDeleted}, categories: {CategoriesDeleted})",
            pointsDeleted,
            daysDeleted,
            categoriesDeleted);
    }

    private async Task RestoreDbSetsInternal(AppDbContext db, BackupFile backup)
    {
        // Everything is new, so there is nothing for change detection to find, and scanning the whole graph on every call is most of the cost.
        var autoDetectChanges = db.ChangeTracker.AutoDetectChangesEnabled;
        db.ChangeTracker.AutoDetectChangesEnabled = false;

        try
        {
            db.Categories.AddRange(backup.Categories);
            db.Days.AddRange(backup.Days);
            db.Points.AddRange(backup.Points);
            await db.SaveChangesAsync();
        }
        finally
        {
            db.ChangeTracker.AutoDetectChangesEnabled = autoDetectChanges;
        }
    }

    public void SetPreferences(BackupFile backup)
    {
        ArgumentNullException.ThrowIfNull(backup);
        ArgumentNullException.ThrowIfNull(backup.PreferenceBackups);

        logger.LogDebug("Restoring {PreferenceCount} preferences from backup", backup.PreferenceBackups.Count);

        // A hand-edited or damaged backup can repeat or omit names, and by now the data is already replaced, so take what's usable instead of throwing.
        var restored = new Dictionary<string, string>();
        foreach (var item in backup.PreferenceBackups)
        {
            if (!string.IsNullOrEmpty(item?.Name))
                restored[item.Name] = item.Value ?? string.Empty;
        }

        preferences.Set(restored);
    }

    /// <summary>
    /// The preferences worth carrying to another device. Ones that were never set are left out so they keep their default there.
    /// </summary>
    public IEnumerable<PreferenceBackup> GetPreferenceBackups() => preferences.GetBackupValues().Select(x => new PreferenceBackup(x.Key, x.Value));

    public async Task<BackupFile> CreateBackup()
    {
        var sw = Stopwatch.StartNew();
        await using var db = await dbFactory.CreateDbContextAsync();

        // Load each table once and let the change tracker wire the points into their days and categories, instead of joining the points in twice.
        var days = await db.Days.ToListAsync();
        var categories = await db.Categories.ToListAsync();
        var points = await db.Points.ToListAsync();

        // Nothing here gets modified, and with detection on, each entry lookup below would rescan every tracked entity.
        db.ChangeTracker.AutoDetectChangesEnabled = false;

        // The collections are complete, so mark them loaded or the proxies would try to lazy load them after this context is gone.
        foreach (var entry in db.ChangeTracker.Entries<Day>())
            entry.Collection(x => x.Points).IsLoaded = true;

        foreach (var entry in db.ChangeTracker.Entries<DataPointCategory>())
            entry.Collection(x => x.Points).IsLoaded = true;

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
            FormatVersion = BackupFile.CurrentFormatVersion,
            AppVersion = ThisAssembly.AssemblyInformationalVersion,
            Days = days,
            Categories = categories,
            Points = points,
            PreferenceBackups = preferencesBackup,
        };
    }
}
