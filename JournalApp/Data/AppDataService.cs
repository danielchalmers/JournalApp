using CommunityToolkit.Maui.Storage;
using MudBlazor;

namespace JournalApp;

public class AppDataService(ILogger<AppDataService> logger, IDialogService dialogService)
{
    public DateTimeOffset LastExportDate => DateTimeOffset.Parse(Preferences.Get("last_export", DateTimeOffset.Now.ToString()));

    public async Task StartImport()
    {
        if (await dialogService.ShowMessageBox(string.Empty, "Importing data will overwrite ALL existing notes, categories, medications!", yesText: "OK", cancelText: "Cancel") == null)
            return;

        // Warn if an export wasn't done in the last week.
        if (DateTimeOffset.Now > LastExportDate.AddDays(7) && await dialogService.ShowMessageBox(string.Empty, "It's recommended to export your data first", yesText: "Continue anyway", cancelText: "Go back") == null)
            return;
    }

    public async Task StartExport()
    {
        using var stream = new MemoryStream(System.Text.Encoding.Default.GetBytes("JournalApp"));
        var fileSaverResult = await FileSaver.Default.SaveAsync($"backup-{DateTime.Now:s}.journalapp", stream, CancellationToken.None);
        if (!fileSaverResult.IsSuccessful)
        {
            logger.LogInformation($"Couldn't save: {fileSaverResult}");
            await dialogService.ShowMessageBox(string.Empty, "Was unable to save the file to your device.");
        }

        logger.LogInformation($"Saved: {fileSaverResult}");
        Preferences.Set("last_export", DateTimeOffset.Now.ToString());
    }

    public async Task ShowExportReminderIfDue()
    {
        if (LastExportDate.AddDays(90) > DateTimeOffset.Now)
            return;

        logger.LogInformation($"It's been a while since the last export <{LastExportDate}>");

        await dialogService.ShowMessageBox(string.Empty, "Keep your data backed up regularly in case something happens to your device by going to the dots menu and choosing \"Export...\"");
    }
}
