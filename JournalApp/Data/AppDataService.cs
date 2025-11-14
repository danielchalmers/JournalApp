namespace JournalApp;

/// <summary>
/// Provides services for managing app data, including backup and restore operations.
/// </summary>
public sealed class AppDataService(ILogger<AppDataService> logger, IDbContextFactory<AppDbContext> dbFactory, PreferenceService preferences)
{
    public async Task DeleteDbSets(CancellationToken cancellationToken = default)
    {
        var sw = Stopwatch.StartNew();

        await using var db = await dbFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        await DeleteDbSetsInternal(db, cancellationToken).ConfigureAwait(false);
        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        sw.Stop();
        logger.LogInformation("Cleared data sets in {ElapsedMilliseconds}ms", sw.ElapsedMilliseconds);
    }

    public async Task RestoreDbSets(BackupFile backup, CancellationToken cancellationToken = default)
    {
        var sw = Stopwatch.StartNew();

        await using var db = await dbFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        await RestoreDbSetsInternal(db, backup, cancellationToken).ConfigureAwait(false);

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
    /// </summary>
    public async Task ReplaceDbSets(BackupFile backup, CancellationToken cancellationToken = default)
    {
        var sw = Stopwatch.StartNew();

        await using var db = await dbFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
        
        try
        {
            // Delete all existing data
            await DeleteDbSetsInternal(db, cancellationToken).ConfigureAwait(false);
            
            // Restore from backup
            await RestoreDbSetsInternal(db, backup, cancellationToken).ConfigureAwait(false);

            // Commit the transaction - both delete and restore succeed or fail together
            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);

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
            await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
            throw;
        }
    }

    private async Task DeleteDbSetsInternal(AppDbContext db, CancellationToken cancellationToken = default)
    {
        var pointsDeleted = await db.Points.ExecuteDeleteAsync(cancellationToken).ConfigureAwait(false);
        var daysDeleted = await db.Days.ExecuteDeleteAsync(cancellationToken).ConfigureAwait(false);
        var categoriesDeleted = await db.Categories.ExecuteDeleteAsync(cancellationToken).ConfigureAwait(false);
        
        logger.LogDebug(
            "Cleared data sets (points: {PointsDeleted}, days: {DaysDeleted}, categories: {CategoriesDeleted})",
            pointsDeleted,
            daysDeleted,
            categoriesDeleted);
    }

    private async Task RestoreDbSetsInternal(AppDbContext db, BackupFile backup, CancellationToken cancellationToken = default)
    {
        await db.Days.AddRangeAsync(backup.Days, cancellationToken).ConfigureAwait(false);
        await db.Categories.AddRangeAsync(backup.Categories, cancellationToken).ConfigureAwait(false);
        await db.Points.AddRangeAsync(backup.Points, cancellationToken).ConfigureAwait(false);
        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
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

    public async Task<BackupFile> CreateBackup(CancellationToken cancellationToken = default)
    {
        var sw = Stopwatch.StartNew();
        await using var db = await dbFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        var days = await db.Days.Include(d => d.Points).ToListAsync(cancellationToken).ConfigureAwait(false);
        var categories = await db.Categories.Include(c => c.Points).ToListAsync(cancellationToken).ConfigureAwait(false);
        var points = await db.Points.ToListAsync(cancellationToken).ConfigureAwait(false);
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
