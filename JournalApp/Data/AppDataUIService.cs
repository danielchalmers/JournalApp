using MudBlazor;

namespace JournalApp;

/// <summary>
/// Provides UI-related services for managing app data, including import and export wizards.
/// </summary>
public sealed class AppDataUIService(
    ILogger<AppDataUIService> logger,
    AppDataService appDataService,
    ExportWizard exportWizard,
    PreferenceService preferenceService)
{
    public async Task<bool> StartImportWizard(IDialogService dialogService, string path)
    {
        logger.LogInformation("Starting import wizard");
        var total = Stopwatch.StartNew();

        // Warn if an export wasn't done in the last week.
        if (DateTimeOffset.Now > preferenceService.LastExportDate.AddDays(7) &&
            await dialogService.ShowJaMessageBox("It's recommended to export your data first in case there are any issues; You can do this in Settings.", yesText: "Continue anyway", cancelText: "Go back") == null)
        {
            total.Stop();
            logger.LogDebug("Import cancelled after export warning");
            return false;
        }

        logger.LogInformation("Reading backup archive from {Path}", path);
        var readStopwatch = Stopwatch.StartNew();

        // Attempt to read the file and its archive.
        BackupFile backup;
        try
        {
            backup = await BackupFile.ReadArchive(path);
            readStopwatch.Stop();
            logger.LogDebug("Archive read successfully in {ElapsedMilliseconds}ms", readStopwatch.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            readStopwatch.Stop();
            total.Stop();
            logger.LogWarning(
                ex,
                "Failed to read archive in {ElapsedMilliseconds}ms (total {TotalElapsedMilliseconds}ms)",
                readStopwatch.ElapsedMilliseconds,
                total.ElapsedMilliseconds);
            await dialogService.ShowJaMessageBox($"Nothing happened; Failed to read archive: {ex.Message}.");
            return false;
        }

        // Warn the user of what's going to happen.
        readStopwatch.Restart();
        if (await dialogService.ShowJaMessageBox(
            $"The selected backup contains {backup.Days.Count} days, {backup.Categories.Count} categories/medications, {backup.Points.Count} points, and {backup.PreferenceBackups.Count} preferences. " +
            "This will replace ALL existing data, cannot be undone, and may take a few minutes.",
            yesText: "Import data", cancelText: "Cancel") == null)
        {
            total.Stop();
            logger.LogDebug("Import cancelled after confirmation dialog ({ElapsedMilliseconds}ms wait)", readStopwatch.ElapsedMilliseconds);
            return false;
        }

        // Restore preferences.
        appDataService.SetPreferences(backup);

        try
        {
            // Import data from backup.
            await appDataService.DeleteDbSets();
            await appDataService.RestoreDbSets(backup);
        }
        catch (Exception ex)
        {
            total.Stop();
            logger.LogError(
                ex,
                "Import failed during database changes after {ElapsedMilliseconds}ms (total {TotalElapsedMilliseconds}ms)",
                readStopwatch.ElapsedMilliseconds,
                total.ElapsedMilliseconds);
            await dialogService.ShowJaMessageBox($"Import critically failed; Database is potentially corrupt and app may need to be reinstalled due to error: {ex}.");
            return false;
        }

        preferenceService.LastExportDate = DateTimeOffset.Now;
        total.Stop();
        logger.LogInformation("Finished import wizard in {ElapsedMilliseconds}ms", total.ElapsedMilliseconds);
        return true;
    }

    [SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "All platforms are supported or not relevant")]
    public async Task StartExportWizard(IDialogService dialogService)
    {
        logger.LogInformation("Starting export wizard UI");

        // Execute the export wizard
        var result = await exportWizard.ExecuteAsync();

        // Handle the result
        if (result.IsSuccessful)
        {
            logger.LogInformation("Export wizard completed successfully");
            // Success - no need to show a message, file is saved
        }
        else if (result.WasCancelled)
        {
            logger.LogInformation("Export cancelled by user");
            // Cancelled - no need to show a message
        }
        else if (result.Exception != null)
        {
            logger.LogWarning(result.Exception, "Export wizard failed");
            await dialogService.ShowJaMessageBox($"Failed to export data: {result.Exception.Message}");
        }
    }
}
