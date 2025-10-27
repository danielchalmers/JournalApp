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
            logger.LogInformation("Starting export wizard");
            var total = Stopwatch.StartNew();

            logger.LogDebug("Constructing backup data");
            var sw = Stopwatch.StartNew();

            var backupFile = await appDataService.CreateBackup();

            logger.LogDebug("Backup data constructed in {ElapsedMilliseconds}ms", sw.ElapsedMilliseconds);
            sw.Restart();

            // Create a memory stream to write the archive to
            using var memoryStream = new MemoryStream();
            try
            {
                await backupFile.WriteArchive(memoryStream);
                memoryStream.Position = 0;

                logger.LogDebug("Archive created in memory in {ElapsedMilliseconds}ms", sw.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                total.Stop();
                logger.LogWarning(
                    ex,
                    "Failed to create archive after {ElapsedMilliseconds}ms (total {TotalElapsedMilliseconds}ms)",
                    sw.ElapsedMilliseconds,
                    total.ElapsedMilliseconds);
                await dialogService.ShowJaMessageBox($"Nothing happened; Failed to create archive: {ex.Message}.");
                return;
            }

            // Prompt the user to save the file.
            sw.Restart();
            try
            {
                var fileName = $"backup-{DateTime.Now:yyyy-MM-dd}.journalapp";
                var result = await fileSaver.SaveAsync(fileName, memoryStream);

                if (result.IsSuccessful)
                {
                    logger.LogDebug("File saved to {FilePath} in {ElapsedMilliseconds}ms", result.FilePath, sw.ElapsedMilliseconds);
                    preferenceService.LastExportDate = DateTimeOffset.Now;
                }
                else
                {
                    logger.LogDebug("File save cancelled or failed after {ElapsedMilliseconds}ms: {Exception}", sw.ElapsedMilliseconds, result.Exception?.Message);
                }
            }
            catch (Exception ex)
            {
                total.Stop();
                logger.LogWarning(
                    ex,
                    "Failed to save file after {ElapsedMilliseconds}ms (total {TotalElapsedMilliseconds}ms)",
                    sw.ElapsedMilliseconds,
                    total.ElapsedMilliseconds);
                await dialogService.ShowJaMessageBox($"Failed to save file: {ex.Message}.");
                return;
            }

            total.Stop();
            logger.LogInformation("Finished export wizard in {ElapsedMilliseconds}ms", total.ElapsedMilliseconds);
        }
}
