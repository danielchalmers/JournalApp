using MudBlazor;

namespace JournalApp;

public sealed class AppDataUIService(ILogger<AppDataUIService> logger, AppDataService appDataService, IShare share, PreferenceService preferenceService)
{
    public async Task<bool> StartImportWizard(IDialogService dialogService, string path)
    {
        logger.LogInformation("Starting import wizard");

        // Warn if an export wasn't done in the last week.
        if (DateTimeOffset.Now > preferenceService.LastExportDate.AddDays(7) &&
            await dialogService.ShowJaMessageBox("It's recommended to export your data first in case there are any issues; You can do this in Settings.", yesText: "Continue anyway", cancelText: "Go back") == null)
        {
            logger.LogDebug("User didn't want to import after being warned about export");
            return false;
        }

        logger.LogInformation($"Reading file: {path}");
        var sw = Stopwatch.StartNew();

        // Attempt to read the file and its archive.
        BackupFile backup;
        try
        {
            backup = await BackupFile.ReadArchive(path);
            logger.LogDebug($"Archive was read successfully after {sw.ElapsedMilliseconds}ms");
        }
        catch (Exception ex)
        {
            logger.LogInformation(ex, $"Failed to read archive after {sw.ElapsedMilliseconds}ms");
            await dialogService.ShowJaMessageBox($"Nothing happened; Failed to read archive: {ex.Message}.");
            return false;
        }

        // Warn the user of what's going to happen.
        sw.Restart();
        if (await dialogService.ShowJaMessageBox(
            $"The selected backup contains {backup.Days.Count} days, {backup.Categories.Count} categories/medications, {backup.Points.Count} points, and {backup.PreferenceBackups.Count} preferences. " +
            "This will replace ALL existing data, cannot be undone, and may take a few minutes.",
            yesText: "Import data", cancelText: "Cancel") == null)
        {
            logger.LogDebug($"User declined to import data after {sw.ElapsedMilliseconds}ms");
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
            logger.LogError(ex, $"Import failed during database changes after {sw.ElapsedMilliseconds}ms");
            await dialogService.ShowJaMessageBox($"Import critically failed; Database is potentially corrupt and app may need to be reinstalled due to error: {ex}.");
            return false;
        }

        preferenceService.LastExportDate = DateTimeOffset.Now;
        logger.LogInformation("Finished import");
        return true;
    }

    [SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "All platforms are supported or not relevant")]
    public async Task StartExportWizard(IDialogService dialogService)
    {
        logger.LogInformation("Starting export wizard");

        logger.LogDebug("Constructing backup data");
        var path = Path.Combine(Path.GetTempPath(), $"backup-{DateTime.Now:yyyy-MM-dd}.journalapp");
        var sw = Stopwatch.StartNew();

        var backupFile = await appDataService.CreateBackup();

        // Create the file and write the archive to it.
        // We don't write directly to a place the user picks because that requires the harsh WRITE_EXTERNAL_STORAGE permission.
        logger.LogDebug($"Creating {path} after {sw.ElapsedMilliseconds}ms");
        sw.Restart();
        try
        {
            await backupFile.WriteArchive(path);

            logger.LogDebug($"File created and archive written in {sw.ElapsedMilliseconds}ms");
        }
        catch (Exception ex)
        {
            logger.LogInformation(ex, $"Failed to create archive after {sw.ElapsedMilliseconds}ms");
            await dialogService.ShowJaMessageBox($"Nothing happened; Failed to create archive: {ex.Message}.");
            return;
        }

        // Prompt the user to share the file.
        await ShareFile(path, "JournalApp backup");

        preferenceService.LastExportDate = DateTimeOffset.Now;
        logger.LogInformation("Finished export");
    }

    public Task ShareFile(string path, string title)
    {
        logger.LogDebug($"Share request <{title}>");

        return share.RequestAsync(new ShareFileRequest
        {
            Title = title,
            File = new ShareFile(path)
        });
    }
}
