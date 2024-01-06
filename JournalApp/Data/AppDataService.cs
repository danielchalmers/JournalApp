using CommunityToolkit.Maui.Storage;
using MudBlazor;

namespace JournalApp;

public class AppDataService(ILogger<AppDataService> logger, AppDbContext db)
{
    public DateTimeOffset LastExportDate => DateTimeOffset.Parse(Preferences.Get("last_export", DateTimeOffset.Now.ToString()));

    public async Task StartImportWizard(IDialogService dialogService)
    {
        // Warn the user of what's going to happen.
        if (await dialogService.ShowMessageBox(string.Empty, "Importing data will replace ALL existing notes, categories, medications, etc, and cannot be undone!", yesText: "OK", cancelText: "Cancel") == null)
            return;

        // Warn if an export wasn't done in the last week.
        if (DateTimeOffset.Now > LastExportDate.AddDays(7) && await dialogService.ShowMessageBox(string.Empty, "It's recommended to export your data first", yesText: "Continue anyway", cancelText: "Go back") == null)
            return;

        // Let user pick the file to import.
        logger.LogInformation("Picking file to import");
        var pickResult = await FilePicker.Default.PickAsync();
        logger.LogInformation($"Picked file: {pickResult.FullPath}");

        if (pickResult == null)
            return;

        logger.LogInformation("Reading file for import");
        await using var stream = await pickResult.OpenReadAsync();

        BackupFile backupFile;
        try
        {
            backupFile = await BackupFile.ReadArchive(stream);
        }
        catch (Exception ex)
        {
            logger.LogDebug($"Failed to read archive: {ex.Message}");
            await dialogService.ShowMessageBox(string.Empty, $"Nothing happened; Failed to read archive: {ex.Message}.");
            return;
        }

        logger.LogDebug("Archive was read");

        foreach (var (key, value) in backupFile.PreferenceBackups)
            Preferences.Set(key, value);
        logger.LogDebug("Set preferences");

        db.SaveChanges();
        logger.LogDebug("Saved database to prepare");

        db.Days.RemoveRange(db.Days);
        db.Categories.RemoveRange(db.Categories);
        db.Points.RemoveRange(db.Points);
        //db.ChangeTracker.Clear();
        //db.Database.Migrate();
        db.SaveChanges();
        logger.LogDebug("Removed old db sets");

        // TODO: Make importing over the same dataset you exported from work.

        db.Days.AddRange(backupFile.Days);
        db.Categories.AddRange(backupFile.Categories);
        db.Points.AddRange(backupFile.Points);
        //db.ChangeTracker.Clear();
        db.SaveChanges();
        logger.LogDebug("Added new db sets");

        logger.LogInformation("Finished import");
    }

    [SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "All platforms are supported or not relevant")]
    public async Task StartExportWizard(IDialogService dialogService)
    {
        var preferenceBackups = new List<PreferenceBackup>();
        foreach (var key in new[] { "safety_plan", "mood_palette" })
            preferenceBackups.Add(new(key, Preferences.Get(key, string.Empty)));

        var backupFile = new BackupFile
        {
            Days = db.Days,
            Categories = db.Categories,
            Points = db.Points,
            PreferenceBackups = preferenceBackups,
        };

        await using var ms = new MemoryStream();

        try
        {
            await backupFile.WriteArchive(ms);
        }
        catch (Exception ex)
        {
            logger.LogDebug($"Failed to create archive: {ex.Message}");
            await dialogService.ShowMessageBox(string.Empty, $"Nothing happened; Failed to create archive: {ex.Message}.");
            return;
        }

        logger.LogInformation("Archive was written");

        // Save the file.
        var saveResult = await FileSaver.Default.SaveAsync($"backup-{DateTime.Now:s}.journalapp", ms, CancellationToken.None);
        logger.LogInformation($"Save status: {saveResult}");

        // Alert user that the file wasn't saved.
        if (!saveResult.IsSuccessful)
        {
            await dialogService.ShowMessageBox(string.Empty, $"Didn't save: {saveResult}.");
            return;
        }

        Preferences.Set("last_export", DateTimeOffset.Now.ToString());
    }

    public async Task ShowExportReminderIfDue(IDialogService dialogService)
    {
        if (LastExportDate.AddDays(90) > DateTimeOffset.Now)
            return;

        logger.LogInformation($"It's been a while since the last export <{LastExportDate}>");

        await dialogService.ShowMessageBox(string.Empty, "Keep your data backed up regularly in case something happens to your device by going to the dots menu and choosing \"Export...\"");
    }
}
