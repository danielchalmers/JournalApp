using CommunityToolkit.Maui.Storage;
using MudBlazor;

namespace JournalApp;

public class AppDataService(ILogger<AppDataService> logger, IDbContextFactory<AppDbContext> dbcf, IFilePicker filePicker, IFileSaver fileSaver)
{
    public async Task<bool> StartImportWizard(IDialogService dialogService)
    {
        // Warn the user of what's going to happen.
        if (await dialogService.ShowMessageBox(string.Empty, "Importing data will replace ALL existing notes, categories, medications, etc, and cannot be undone!", yesText: "OK", cancelText: "Cancel") == null)
            return false;

        // Warn if an export wasn't done in the last week.
        if (DateTimeOffset.Now > LastExportDate.AddDays(7) && await dialogService.ShowMessageBox(string.Empty, "It's recommended to export your data first", yesText: "Continue anyway", cancelText: "Go back") == null)
            return false;

        // Let user pick the file to import.
        logger.LogInformation("Picking file to import");
        var pickResult = await filePicker.PickAsync();

        if (pickResult == null)
        {
            logger.LogInformation("Didn't pick file");
            return false;
        }

        logger.LogInformation($"Reading file: {pickResult.FullPath}");
        await using var stream = await pickResult.OpenReadAsync();

        // Attempt to read the archive.
        BackupFile backupFile;
        try
        {
            backupFile = await BackupFile.ReadArchive(stream);
        }
        catch (Exception ex)
        {
            logger.LogInformation(ex, "Failed to read archive");
            await dialogService.ShowMessageBox(string.Empty, $"Nothing happened; Failed to read archive: {ex.Message}.");
            return false;
        }

        logger.LogDebug("Archive was read");

        // Set preferences.
        foreach (var (key, value) in backupFile.PreferenceBackups)
        {
            Preferences.Set(key, value);
            logger.LogInformation($"Preference set: {key}");
        }

        // Apply the backup content to the database.
        await using (var db = await dbcf.CreateDbContextAsync())
        {
            db.SaveChanges();
            logger.LogDebug("Saved database to prepare");

            db.Days.RemoveRange(db.Days);
            db.Categories.RemoveRange(db.Categories);
            db.Points.RemoveRange(db.Points);
            db.SaveChanges();
            logger.LogDebug("Cleared old db sets");

            db.Days.AddRange(backupFile.Days);
            db.Categories.AddRange(backupFile.Categories);
            db.Points.AddRange(backupFile.Points);
            db.SaveChanges();
            logger.LogDebug("Added new data");
        }

        logger.LogInformation("Finished import");
        return true;
    }

    [SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "All platforms are supported or not relevant")]
    public async Task StartExportWizard(IDialogService dialogService)
    {
        var preferenceBackups = new List<PreferenceBackup>();
        foreach (var key in new[] { "safety_plan", "mood_palette" })
            preferenceBackups.Add(new(key, Preferences.Get(key, string.Empty)));

        await using var db = await dbcf.CreateDbContextAsync();

        var backupFile = new BackupFile
        {
            Days = db.Days,
            Categories = db.Categories,
            Points = db.Points,
            PreferenceBackups = preferenceBackups,
        };

        // Initialize the memory stream that will be used when saving.
        await using var ms = new MemoryStream();

        // Attempt to create the archive.
        try
        {
            await backupFile.WriteArchive(ms);
        }
        catch (Exception ex)
        {
            logger.LogInformation(ex, "Failed to create archive");
            await dialogService.ShowMessageBox(string.Empty, $"Nothing happened; Failed to create archive: {ex.Message}.");
            return;
        }

        logger.LogDebug("Archive was written");

        // Save the file through a prompt.
        var saverResult = await fileSaver.SaveAsync($"backup-{DateTime.Now:s}.journalapp", ms, CancellationToken.None);
        logger.LogInformation($"File saver result: {saverResult}");

        // Alert user that the file wasn't saved.
        if (!saverResult.IsSuccessful)
        {
            await dialogService.ShowMessageBox(string.Empty, $"Didn't save: {saverResult.Exception.Message}");
            return;
        }

        Preferences.Set("last_export", DateTimeOffset.Now.ToString());
    }

    public async Task ShowExportReminderIfDue(IDialogService dialogService)
    {
        if (LastExportDate.AddDays(180) > DateTimeOffset.Now)
            return;

        logger.LogInformation($"It's been a while since last export <{LastExportDate}>");

        // We're going to show the message so let's not bug the user again until next interval.
        LastExportDate = DateTimeOffset.Now;

        await dialogService.ShowMessageBox(string.Empty, "Reminder: You haven't backed your data up in a while. You can do this by going to the dots menu and choosing \"Export\".");
    }

    public DateTimeOffset LastExportDate
    {
        get
        {
            var lastExportString = Preferences.Get("last_export", null);

            if (DateTimeOffset.TryParse(lastExportString, out var parsed))
            {
                return parsed;
            }
            else
            {
                // If we haven't tracked this, or it's malformed, set it to current so the user won't immediately get notified after first launch.
                LastExportDate = DateTimeOffset.Now;
                return DateTimeOffset.Now;
            }
        }
        set => Preferences.Set("last_export", DateTimeOffset.Now.ToString("O"));
    }
}
