using CommunityToolkit.Maui.Storage;
using MudBlazor;

namespace JournalApp;

public class AppDataService
{
    private readonly ILogger<AppDataService> _logger;
    private readonly IDialogService _dialogService;

    public AppDataService(ILogger<AppDataService> logger, IDialogService dialogService)
    {
        _logger = logger;
        _dialogService = dialogService;
    }

    public DateTimeOffset LastExportDate => DateTimeOffset.Parse(Preferences.Get("last_export", DateTimeOffset.Now.ToString()));

    public async Task StartImport()
    {
        if (await _dialogService.ShowMessageBox(string.Empty, "Importing data will overwrite ALL existing notes, categories, medications!", yesText: "OK", cancelText: "Cancel") == null)
            return;

        // Warn if an export wasn't done in the last week.
        if (DateTimeOffset.Now > LastExportDate.AddDays(7) && await _dialogService.ShowMessageBox(string.Empty, "It's recommended to export your data first", yesText: "Continue anyway", cancelText: "Go back") == null)
            return;
    }

    public async Task StartExport()
    {
        using var stream = new MemoryStream(System.Text.Encoding.Default.GetBytes("JournalApp"));
        var fileSaverResult = await FileSaver.Default.SaveAsync($"backup-{DateTime.Now:s}.journalapp", stream, CancellationToken.None);
        if (!fileSaverResult.IsSuccessful)
        {
            _logger.LogInformation($"Couldn't save: {fileSaverResult}");
            await _dialogService.ShowMessageBox(string.Empty, "Was unable to save the file to your device.");
        }

        _logger.LogInformation($"Saved: {fileSaverResult}");
        Preferences.Set("last_export", DateTimeOffset.Now.ToString());
    }

    public async Task ShowExportReminderIfDue()
    {
        if (LastExportDate.AddDays(90) > DateTimeOffset.Now)
            return;

        _logger.LogInformation($"It's been a while since the last export <{LastExportDate}>");

        await _dialogService.ShowMessageBox(string.Empty, "Keep your data backed up regularly in case something happens to your device by going to the dots menu and choosing \"Export...\"");
    }
}
