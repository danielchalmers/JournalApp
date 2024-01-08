using CommunityToolkit.Maui.Storage;
using MudBlazor;

namespace JournalApp;

public class AppDataService(ILogger<AppDataService> logger, IDbContextFactory<AppDbContext> dbcf)
{
    public DateTimeOffset LastExportDate => DateTimeOffset.Parse(Preferences.Get("last_export", DateTimeOffset.Now.ToString()));

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
        var pickResult = await FilePicker.Default.PickAsync();

        if (pickResult == null)
            return false;

        logger.LogInformation($"Reading file: {pickResult.FullPath}");
        await using var stream = await pickResult.OpenReadAsync();

        BackupFile backupFile;
        try
        {
            backupFile = await BackupFile.ReadArchive(stream);
        }
        catch (Exception ex)
        {
            logger.LogDebug(ex, "Failed to read archive");
            await dialogService.ShowMessageBox(string.Empty, $"Nothing happened; Failed to read archive: {ex.Message}.");
            return false;
        }

        logger.LogDebug("Archive was read");

        foreach (var (key, value) in backupFile.PreferenceBackups)
            Preferences.Set(key, value);
        logger.LogDebug("Preferences were set");

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

        await using var ms = new MemoryStream();

        try
        {
            await backupFile.WriteArchive(ms);
        }
        catch (Exception ex)
        {
            logger.LogDebug(ex, "Failed to create archive");
            await dialogService.ShowMessageBox(string.Empty, $"Nothing happened; Failed to create archive: {ex.Message}.");
            return;
        }

        logger.LogInformation("Archive was written");

        // Save the file.
        var saveResult = await FileSaver.Default.SaveAsync($"backup-{DateTime.Now:s}.journalapp", ms, CancellationToken.None);
        logger.LogInformation($"Save picker result: {saveResult}");

        // Alert user that the file wasn't saved.
        if (!saveResult.IsSuccessful)
        {
            await dialogService.ShowMessageBox(string.Empty, $"Didn't save: {saveResult.Exception.Message}");
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
