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
            backup = await BackupFile.ReadArchive(path);
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
            await dialogService.ShowJaMessageBox($"Import failed: {ex.Message}");
            return false;
        }

        // Warn the user of what's going to happen.
        readStopwatch.Restart();
        if (await dialogService.ShowJaMessageBox(
            $"Contains {backup.Days.Count} days, {backup.Categories.Count} categories, {backup.Points.Count} points, {backup.PreferenceBackups.Count} preferences.\n\n" +
            "⚠️ This will replace ALL current data and cannot be undone.",
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
            await appDataService.ReplaceDbSets(backup);

            // Only restore preferences after database operations succeed.
            appDataService.SetPreferences(backup);
        }
        catch (Exception ex)
        {
            total.Stop();
            logger.LogError(
                ex,
                "Import failed during database operations after {ElapsedMilliseconds}ms (total {TotalElapsedMilliseconds}ms)",
                readStopwatch.ElapsedMilliseconds,
                total.ElapsedMilliseconds);
            await dialogService.ShowJaMessageBox($"Import failed: {ex.Message}");
            return false;
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

        BackupFile backupFile;
        byte[] archiveBytes;

        try
        {
            var sw = Stopwatch.StartNew();

            backupFile = await appDataService.CreateBackup();
            logger.LogInformation("Backup created in {ElapsedMilliseconds}ms - {DayCount} days, {CategoryCount} categories, {PointCount} points",
                sw.ElapsedMilliseconds, backupFile.Days.Count, backupFile.Categories.Count, backupFile.Points.Count);

            sw.Restart();

            using (var memoryStream = new MemoryStream())
            {
                await backupFile.WriteArchive(memoryStream);
                archiveBytes = memoryStream.ToArray();
            }

            logger.LogInformation("Archive created ({SizeKB} KB) in {ElapsedMilliseconds}ms",
                archiveBytes.Length / 1024, sw.ElapsedMilliseconds);
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
            using var saveStream = new MemoryStream(archiveBytes);
            var fileName = $"backup-{DateTime.Now:yyyy-MM-dd}.journalapp";
            var result = await fileSaver.SaveAsync(fileName, saveStream);

            if (result.IsSuccessful)
            {
                logger.LogInformation("File saved to {FilePath}", result.FilePath);
                preferenceService.LastExportDate = DateTimeOffset.Now;
                total.Stop();
                logger.LogInformation("Export completed in {TotalElapsedMilliseconds}ms", total.ElapsedMilliseconds);
            }
            else if (result.Exception != null)
            {
                total.Stop();
                logger.LogError(result.Exception, "File save failed");
                await dialogService.ShowJaMessageBox($"Export failed: {result.Exception.Message}");
            }
            else
            {
                total.Stop();
                logger.LogInformation("Export cancelled by user");
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
