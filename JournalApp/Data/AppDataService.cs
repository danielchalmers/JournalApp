using MudBlazor;

namespace JournalApp;

public class AppDataService(ILogger<AppDataService> logger, IDbContextFactory<AppDbContext> dbcf, IFilePicker filePicker, IShare share)
{
    public async Task<bool> StartImportWizard(IDialogService dialogService)
    {
        logger.LogInformation("Starting import wizard");

        // Warn the user of what's going to happen.
        if (await dialogService.ShowCustomMessageBox(string.Empty, "Importing data will replace ALL existing notes, categories, medications, etc, and cannot be undone!", yesText: "OK", cancelText: "Cancel") == null)
        {
            logger.LogDebug("User declined to start importing data");
            return false;
        }

        // Warn if an export wasn't done in the last week.
        if (DateTimeOffset.Now > LastExportDate.AddDays(7) && await dialogService.ShowCustomMessageBox(string.Empty, "It's recommended to export your data first", yesText: "Continue anyway", cancelText: "Go back") == null)
        {
            logger.LogDebug("User didn't want to import and might export first");
            return false;
        }

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
            await db.SaveChangesAsync();
            logger.LogDebug("Cleared old db sets");
        }

        await using (var db = await dbcf.CreateDbContextAsync())
        {
            await db.Days.AddRangeAsync(backupFile.Days);
            await db.Categories.AddRangeAsync(backupFile.Categories);
            await db.Points.AddRangeAsync(backupFile.Points);
            await db.SaveChangesAsync();
            logger.LogDebug("Added new data");
        }

        logger.LogInformation("Finished import");
        return true;
    }

    [SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "All platforms are supported or not relevant")]
    public async Task StartExportWizard(IDialogService dialogService)
    {
        logger.LogInformation("Starting export wizard");

        // Clean up old backups.
        foreach (var f in Directory.EnumerateFiles(FileSystem.AppDataDirectory, "backup-*.journalapp"))
        {
            try
            {
                File.Delete(f);
                logger.LogDebug($"Deleted old export <{f}>");
            }
            catch (IOException ex)
            {
                logger.LogError(ex, $"Tried to delete old export <{f}>");
            }
        }

        // TODO: Show a message box but close it for the user afterwards if they don't https://github.com/MudBlazor/MudBlazor/issues/8048.
        //_ = dialogService.ShowCustomMessageBox(string.Empty, "Please wait while the archive is created for you", showFeedbackLink: true);

        logger.LogDebug("Constructing backup data");
        var sw = Stopwatch.StartNew();

        var preferenceBackups = new List<PreferenceBackup>();
        foreach (var key in new[] { "safety_plan", "mood_palette" })
            preferenceBackups.Add(new(key, Preferences.Get(key, string.Empty)));

        BackupFile backupFile;
        // TODO: Go from DBSet directly to Stream to avoid memory overhead.
        await using (var db = await dbcf.CreateDbContextAsync())
        {
            db.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

            backupFile = new()
            {
                Days = db.Days.Include(d => d.Points).ToHashSet(),
                Categories = db.Categories.Include(c => c.Points).ToHashSet(),
                Points = db.Points.ToHashSet(),
                PreferenceBackups = preferenceBackups,
            };
        }

        // Create the file and write the archive to it.
        // We don't write directly to a place the user picks because that requires the harsh WRITE_EXTERNAL_STORAGE permission.
        var filePath = Path.Combine(FileSystem.AppDataDirectory, $"backup-{DateTime.Now:yyyy-MM-dd}.journalapp");
        logger.LogDebug($"Creating {filePath} after {sw.ElapsedMilliseconds}ms");
        sw.Restart();
        try
        {
            await using (var stream = File.Create(filePath))
            {
                await backupFile.WriteArchive(stream);

                logger.LogDebug($"File created and archive written in {sw.ElapsedMilliseconds}ms");
            }
        }
        catch (Exception ex)
        {
            logger.LogInformation(ex, $"Failed to create archive after {sw.ElapsedMilliseconds}ms");
            await dialogService.ShowCustomMessageBox(string.Empty, $"Nothing happened; Failed to create archive: {ex.Message}.", showFeedbackLink: true);
            return;
        }

        // Prompt the user to share the file.
        logger.LogDebug("Share request");
        await share.RequestAsync(new ShareFileRequest
        {
            Title = "JournalApp backup",
            File = new ShareFile(filePath)
        });

        Preferences.Set("last_export", DateTimeOffset.Now.ToString());

        logger.LogInformation("Finished export");
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
        set => Preferences.Set("last_export", value.ToString("O"));
    }
}
