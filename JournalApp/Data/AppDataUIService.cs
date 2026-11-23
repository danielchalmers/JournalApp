using CommunityToolkit.Maui.Storage;
using MudBlazor;

namespace JournalApp;

/// <summary>
/// Provides UI-related services for managing app data, including import and export wizards.
/// </summary>
public sealed class AppDataUIService(ILogger<AppDataUIService> logger, AppDataService appDataService, IFileSaver fileSaver, PreferenceService preferenceService)
{
    public async Task<bool> StartImportWizard(IDialogService dialogService, string path)
    {
        logger.LogInformation("Starting import wizard for file: {Path}", path);
        var total = Stopwatch.StartNew();

        // Warn if an export wasn't done in the last week.
        if (DateTimeOffset.Now > preferenceService.LastExportDate.AddDays(7) &&
            await dialogService.ShowJaMessageBox("Recommended: Back up your current data first from Settings.", yesText: "Continue anyway", cancelText: "Go back") == null)
        {
            total.Stop();
            logger.LogInformation("Import cancelled by user after backup recommendation warning");
            return false;
        }

        logger.LogInformation("Reading backup file from {Path}", path);
        var readStopwatch = Stopwatch.StartNew();

        // Attempt to read the file and its archive.
        BackupFile backup;
        try
        {
            // Off the UI thread so the busy spinner can paint. The dialogs below continue back on it.
            backup = await Task.Run(() => BackupFile.ReadArchive(path));
            readStopwatch.Stop();
            logger.LogInformation("Backup file read successfully in {ElapsedMilliseconds}ms - contains {DayCount} days, {CategoryCount} categories, {PointCount} points",
                readStopwatch.ElapsedMilliseconds, backup.Days.Count, backup.Categories.Count, backup.Points.Count);
        }
        catch (Exception ex)
        {
            readStopwatch.Stop();
            total.Stop();
            logger.LogError(
                ex,
                "Failed to read backup file in {ElapsedMilliseconds}ms (total {TotalElapsedMilliseconds}ms)",
                readStopwatch.ElapsedMilliseconds,
                total.ElapsedMilliseconds);
            // Only our own read errors are written for the user. Anything else is the file system, and its text isn't helpful.
            var reason = ex is BackupReadException ? ex.Message : "The file couldn't be opened.";
            await dialogService.ShowJaMessageBox($"Import failed: {reason}");
            return false;
        }

        // Warn the user of what's going to happen.
        var summary = $"Contains {backup.Days.Count} days, {backup.Categories.Count} categories, {backup.Points.Count} points, {backup.PreferenceBackups.Count} preferences.\n\n";

        if (backup.IsFromNewerApp)
        {
            logger.LogWarning("Backup format {FormatVersion} is newer than this app's {CurrentFormatVersion} (written by {AppVersion})", backup.FormatVersion, BackupFile.CurrentFormatVersion, backup.AppVersion);
            summary += "This backup was made by a newer version of the app, so some of it may not import correctly. Updating the app first is safer.\n\n";
        }

        readStopwatch.Restart();
        if (await dialogService.ShowJaMessageBox(
            summary + "⚠️ This will replace ALL current data and cannot be undone.",
            yesText: "Import", cancelText: "Cancel") == null)
        {
            total.Stop();
            logger.LogInformation("Import cancelled by user after confirmation dialog ({ElapsedMilliseconds}ms wait)", readStopwatch.ElapsedMilliseconds);
            return false;
        }

        logger.LogInformation("Starting data import - replacing all existing data with backup");

        try
        {
            // Import data from backup atomically - delete and restore in a single transaction.
            // If this fails, the database will be rolled back to its original state.
            await Task.Run(() => appDataService.ReplaceDbSets(backup));
        }
        catch (Exception ex)
        {
            total.Stop();
            logger.LogError(
                ex,
                "Import failed during database operations after {ElapsedMilliseconds}ms (total {TotalElapsedMilliseconds}ms)",
                readStopwatch.ElapsedMilliseconds,
                total.ElapsedMilliseconds);
            await dialogService.ShowJaMessageBox("Import failed: The backup has data that can't be imported. Your current data was not changed.");
            return false;
        }

        // Only restore preferences after database operations succeed.
        // The data is already committed at this point, so a problem here must not be reported as a failed import.
        try
        {
            appDataService.SetPreferences(backup);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Data was imported but preferences could not be restored");
            await dialogService.ShowJaMessageBox("Your data was imported, but some settings from the backup could not be restored.");
        }

        preferenceService.LastExportDate = DateTimeOffset.Now;
        total.Stop();
        logger.LogInformation("Import completed successfully in {ElapsedMilliseconds}ms", total.ElapsedMilliseconds);
        return true;
    }

    [SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "All platforms are supported or not relevant")]
    public async Task StartExportWizard(IDialogService dialogService)
    {
        logger.LogInformation("Starting export wizard");
        var total = Stopwatch.StartNew();

        using var archiveStream = new MemoryStream();

        try
        {
            // Off the UI thread so the busy spinner can paint. The dialogs below continue back on it.
            await Task.Run(async () =>
            {
                var sw = Stopwatch.StartNew();

                var backupFile = await appDataService.CreateBackup();
                logger.LogInformation("Backup created in {ElapsedMilliseconds}ms - {DayCount} days, {CategoryCount} categories, {PointCount} points",
                    sw.ElapsedMilliseconds, backupFile.Days.Count, backupFile.Categories.Count, backupFile.Points.Count);

                sw.Restart();
                await backupFile.WriteArchive(archiveStream);

                logger.LogInformation("Archive created ({SizeKB} KB) in {ElapsedMilliseconds}ms",
                    archiveStream.Length / 1024, sw.ElapsedMilliseconds);
            });
        }
        catch (Exception ex)
        {
            total.Stop();
            logger.LogError(ex, "Failed to create backup");
            await dialogService.ShowJaMessageBox($"Export failed: {ex.Message}");
            return;
        }

        logger.LogInformation("Prompting user to select save location");

        try
        {
            // Hand the saver the stream the archive was written to instead of copying it.
            archiveStream.Position = 0;

            // Invariant so the name doesn't follow the device calendar (Buddhist, Hijri, ...).
            var fileName = $"backup-{DateTime.Now.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture)}.journalapp";
            var result = await fileSaver.SaveAsync(fileName, archiveStream);

            if (result.IsSuccessful)
            {
                logger.LogInformation("File saved to {FilePath}", result.FilePath);
                preferenceService.LastExportDate = DateTimeOffset.Now;
                total.Stop();
                logger.LogInformation("Export completed in {TotalElapsedMilliseconds}ms", total.ElapsedMilliseconds);
            }
            else if (result.IsCancelled)
            {
                // Backing out of the save dialog comes back as a cancellation exception, not as a result without one.
                total.Stop();
                logger.LogInformation("Export cancelled by user");
            }
            else
            {
                total.Stop();
                logger.LogError(result.Exception, "File save failed");
                await dialogService.ShowJaMessageBox($"Export failed: {result.Exception.Message}");
            }
        }
        catch (Exception ex)
        {
            total.Stop();
            logger.LogError(ex, "Failed to save file");
            await dialogService.ShowJaMessageBox($"Export failed: {ex.Message}");
        }
    }
}
