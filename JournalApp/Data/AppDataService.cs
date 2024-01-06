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
        logger.LogInformation($"Pick result: {pickResult}");

        if (pickResult == null)
            return;

        logger.LogInformation("Reading file for import");
        await using var stream = await pickResult.OpenReadAsync();

        var backupFile = BackupFile.ReadArchive(stream);
        logger.LogDebug("Archive was read");

        foreach (var (key, value) in backupFile.BackupPreferences)
            Preferences.Set(key, value);
        logger.LogDebug("Set preferences");

        db.Days.RemoveRange(db.Days);
        db.Categories.RemoveRange(db.Categories);
        db.Points.RemoveRange(db.Points);

        db.Days.AddRange(backupFile.Days);
        db.Categories.AddRange(backupFile.Categories);
        db.Points.AddRange(backupFile.Points);

        db.SaveChanges();

        logger.LogDebug("Replaced db sets");
    }

    [SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "All platforms are supported or not relevant")]
    public async Task StartExportWizard(IDialogService dialogService)
    {
        var backupFile = new BackupFile
        {
            Days = db.Days,
            Categories = db.Categories,
            Points = db.Points
        };

        // Create a stream and write an archive file.
        await using var ms = new MemoryStream();

        try
        {
            backupFile.WriteArchive(ms);
        }
        catch (Exception ex)
        {
            await dialogService.ShowMessageBox(string.Empty, $"Failed to create archive: {ex.Message}.");
            return;
        }

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
