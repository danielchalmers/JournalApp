using MudBlazor;

namespace JournalApp;

public class AppDataService(ILogger<AppDataService> logger, IDbContextFactory<AppDbContext> dbcf, IFilePicker filePicker, IShare share)
{
    public async Task<bool> StartImportWizard(IDialogService dialogService)
    {
        // Warn the user of what's going to happen.
        if (await dialogService.ShowCustomMessageBox(string.Empty, "Importing data will replace ALL existing notes, categories, medications, etc, and cannot be undone!", yesText: "OK", cancelText: "Cancel") == null)
            return false;

        // Warn if an export wasn't done in the last week.
        if (DateTimeOffset.Now > LastExportDate.AddDays(7) && await dialogService.ShowCustomMessageBox(string.Empty, "It's recommended to export your data first", yesText: "Continue anyway", cancelText: "Go back") == null)
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
            await dialogService.ShowCustomMessageBox(string.Empty, $"Nothing happened; Failed to read archive: {ex.Message}.", showFeedbackLink: true);
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

        // Create the file and write the archive to it.
        // We don't write directly to a place the user picks because that requires the harsh WRITE_EXTERNAL_STORAGE permission.
        var filePath = Path.Combine(FileSystem.AppDataDirectory, $"backup-{DateTime.Now:yyyy-MM-dd}.journalapp");
        logger.LogDebug($"Creating a file at {filePath}");
        try
        {
            await using (var stream = File.Create(filePath))
            {
                await backupFile.WriteArchive(stream);

                logger.LogDebug("Archive was written");
            }
        }
        catch (Exception ex)
        {
            logger.LogInformation(ex, "Failed to create archive");
            await dialogService.ShowCustomMessageBox(string.Empty, $"Nothing happened; Failed to create archive: {ex.Message}.", showFeedbackLink: true);
            return;
        }

        // Prompt the user to share the file.
        await share.RequestAsync(new ShareFileRequest
        {
            Title = "JournalApp backup",
            File = new ShareFile(filePath)
        });

        Preferences.Set("last_export", DateTimeOffset.Now.ToString());
    }

    public async Task ShowExportReminderIfDue(IDialogService dialogService)
    {
        if (LastExportDate.AddDays(180) > DateTimeOffset.Now)
            return;

        logger.LogInformation($"It's been a while since last export <{LastExportDate}>");

        // We're going to show the message so let's not bug the user again until next interval.
        LastExportDate = DateTimeOffset.Now;

        await dialogService.ShowCustomMessageBox(string.Empty, "Reminder: You haven't backed your data up in a while. You can do this by going to the dots menu and choosing \"Export\".");
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
