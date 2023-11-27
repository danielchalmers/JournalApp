using CommunityToolkit.Maui.Storage;
using MudBlazor;

namespace JournalApp;

public class AppDataService(ILogger<AppDataService> logger, AppDbContext db)
{
    public DateTimeOffset LastExportDate => DateTimeOffset.Parse(Preferences.Get("last_export", DateTimeOffset.Now.ToString()));

    public async Task StartImportWizard(IDialogService dialogService)
    {
        // Warn the user of what's going to happen.
        if (await dialogService.ShowMessageBox(string.Empty, "Importing data will overwrite ALL existing notes, categories, medications!", yesText: "OK", cancelText: "Cancel") == null)
            return;

        // Warn if an export wasn't done in the last week.
        if (DateTimeOffset.Now > LastExportDate.AddDays(7) && await dialogService.ShowMessageBox(string.Empty, "It's recommended to export your data first", yesText: "Continue anyway", cancelText: "Go back") == null)
            return;

        // Let user pick the file to import.
        logger.LogInformation("Picking file to import.");
        var pickResult = await FilePicker.Default.PickAsync();
        if (pickResult == null)
        {
            logger.LogInformation("Didn't pick file.");
            return;
        }

        logger.LogInformation($"Picked: {pickResult}");

        // Read the backup file.
        logger.LogInformation("Reading backup file");
        using var stream = await pickResult.OpenReadAsync();

        // Do some integrity checks.

        // File is ok. Reset data before importation.
        ResetData();

        // Import new data.
    }

    [SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "All platforms are supported or not relevant")]
    public async Task StartExportWizard(IDialogService dialogService)
    {
        // Create the stream of file contents.
        using var stream = new MemoryStream(Encoding.Default.GetBytes("JournalApp"));

        // Save the file.
        var saveResult = await FileSaver.Default.SaveAsync($"backup-{DateTime.Now:s}.journalapp", stream, CancellationToken.None);

        // Alert user that the file wasn't saved.
        if (!saveResult.IsSuccessful)
        {
            logger.LogInformation($"Couldn't save: {saveResult}");
            await dialogService.ShowMessageBox(string.Empty, "Was unable to save the file to your device.");
            return;
        }

        logger.LogInformation($"Saved: {saveResult}");
        Preferences.Set("last_export", DateTimeOffset.Now.ToString());
    }

    public async Task ShowExportReminderIfDue(IDialogService dialogService)
    {
        if (LastExportDate.AddDays(90) > DateTimeOffset.Now)
            return;

        logger.LogInformation($"It's been a while since the last export <{LastExportDate}>");

        await dialogService.ShowMessageBox(string.Empty, "Keep your data backed up regularly in case something happens to your device by going to the dots menu and choosing \"Export...\"");
    }

    /// <summary>
    /// Very dangerous funtion that will reset all the data on the app.
    /// </summary>
    private void ResetData()
    {
        logger.LogInformation("Resetting all data.");

        Preferences.Clear();

        db.Days.RemoveRange(db.Days);
        db.Categories.RemoveRange(db.Categories);
        db.Points.RemoveRange(db.Points);

        db.SaveChanges();
    }
}
