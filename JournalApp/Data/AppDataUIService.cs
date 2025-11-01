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
                await dialogService.ShowJaMessageBox("It's recommended to create a backup of your current data before importing. You can do this from the Settings page.", yesText: "Continue anyway", cancelText: "Go back") == null)
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
                await dialogService.ShowJaMessageBox($"Import failed: Could not read the backup file. {ex.Message}");
                return false;
            }

            // Warn the user of what's going to happen.
            readStopwatch.Restart();
            if (await dialogService.ShowJaMessageBox(
                $"The backup file contains {backup.Days.Count} days, {backup.Categories.Count} categories/medications, {backup.Points.Count} points, and {backup.PreferenceBackups.Count} preferences.\n\n" +
                "⚠️ This will permanently replace ALL of your current data and cannot be undone. The import may take a few minutes to complete.",
                yesText: "Import", cancelText: "Cancel") == null)
            {
                total.Stop();
                logger.LogInformation("Import cancelled by user after confirmation dialog ({ElapsedMilliseconds}ms wait)", readStopwatch.ElapsedMilliseconds);
                return false;
            }

            logger.LogInformation("Starting data import - deleting existing data and restoring from backup");

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
                    "Import failed during database operations after {ElapsedMilliseconds}ms (total {TotalElapsedMilliseconds}ms)",
                    readStopwatch.ElapsedMilliseconds,
                    total.ElapsedMilliseconds);
                await dialogService.ShowJaMessageBox($"Import failed: The database could not be updated and may be corrupted. You may need to reinstall the app. Error: {ex.Message}");
                return false;
            }

            preferenceService.LastExportDate = DateTimeOffset.Now;
            total.Stop();
            logger.LogInformation("Import completed successfully in {ElapsedMilliseconds}ms", total.ElapsedMilliseconds);
            await dialogService.ShowJaMessageBox("Your data has been successfully imported!");
            return true;
        }

        [SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "All platforms are supported or not relevant")]
        public async Task StartExportWizard(IDialogService dialogService)
        {
            logger.LogInformation("Starting export wizard");
            var total = Stopwatch.StartNew();

            // Prompt the user first before doing any work
            if (await dialogService.ShowJaMessageBox(
                "This will create a backup file containing all your journal data. Choose where to save it in the next step.",
                yesText: "Continue", cancelText: "Cancel") == null)
            {
                total.Stop();
                logger.LogInformation("Export cancelled by user before creating backup");
                return;
            }

            logger.LogInformation("Preparing backup data for export");
            var sw = Stopwatch.StartNew();

            BackupFile backupFile;
            try
            {
                backupFile = await appDataService.CreateBackup();
                logger.LogInformation("Backup data prepared in {ElapsedMilliseconds}ms - contains {DayCount} days, {CategoryCount} categories, {PointCount} points", 
                    sw.ElapsedMilliseconds, backupFile.Days.Count, backupFile.Categories.Count, backupFile.Points.Count);
            }
            catch (Exception ex)
            {
                total.Stop();
                logger.LogError(
                    ex,
                    "Failed to prepare backup data after {ElapsedMilliseconds}ms",
                    sw.ElapsedMilliseconds);
                await dialogService.ShowJaMessageBox($"Export failed: Could not prepare your data for backup. {ex.Message}");
                return;
            }

            sw.Restart();

            // Create a memory stream to write the archive to
            MemoryStream archiveStream = null;
            try
            {
                archiveStream = new MemoryStream();
                await backupFile.WriteArchive(archiveStream);
                archiveStream.Position = 0; // Reset position to beginning for reading

                logger.LogInformation("Backup archive created in memory ({SizeKB} KB) in {ElapsedMilliseconds}ms", 
                    archiveStream.Length / 1024, sw.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                archiveStream?.Dispose();
                total.Stop();
                logger.LogError(
                    ex,
                    "Failed to create backup archive after {ElapsedMilliseconds}ms (total {TotalElapsedMilliseconds}ms)",
                    sw.ElapsedMilliseconds,
                    total.ElapsedMilliseconds);
                await dialogService.ShowJaMessageBox($"Export failed: Could not create the backup archive. {ex.Message}");
                return;
            }

            // Prompt the user to save the file.
            logger.LogInformation("Prompting user to select save location");
            sw.Restart();
            try
            {
                var fileName = $"backup-{DateTime.Now:yyyy-MM-dd}.journalapp";
                var result = await fileSaver.SaveAsync(fileName, archiveStream);

                if (result.IsSuccessful)
                {
                    logger.LogInformation("Backup file saved successfully to {FilePath} in {ElapsedMilliseconds}ms", 
                        result.FilePath, sw.ElapsedMilliseconds);
                    preferenceService.LastExportDate = DateTimeOffset.Now;
                    total.Stop();
                    logger.LogInformation("Export completed successfully in {TotalElapsedMilliseconds}ms", total.ElapsedMilliseconds);
                    await dialogService.ShowJaMessageBox("Your data has been successfully exported!");
                }
                else if (result.Exception != null)
                {
                    total.Stop();
                    logger.LogError(result.Exception, "File save failed after {ElapsedMilliseconds}ms (total {TotalElapsedMilliseconds}ms)", 
                        sw.ElapsedMilliseconds, total.ElapsedMilliseconds);
                    await dialogService.ShowJaMessageBox($"Export failed: Could not save the backup file. {result.Exception.Message}");
                }
                else
                {
                    total.Stop();
                    logger.LogInformation("Export cancelled by user after {ElapsedMilliseconds}ms (total {TotalElapsedMilliseconds}ms)", 
                        sw.ElapsedMilliseconds, total.ElapsedMilliseconds);
                }
            }
            catch (Exception ex)
            {
                total.Stop();
                logger.LogError(
                    ex,
                    "Failed to save backup file after {ElapsedMilliseconds}ms (total {TotalElapsedMilliseconds}ms)",
                    sw.ElapsedMilliseconds,
                    total.ElapsedMilliseconds);
                await dialogService.ShowJaMessageBox($"Export failed: Could not save the backup file. {ex.Message}");
            }
            finally
            {
                archiveStream?.Dispose();
            }
        }
}
