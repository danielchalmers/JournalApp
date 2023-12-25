using CommunityToolkit.Maui.Storage;
using MudBlazor;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace JournalApp;

public class TrendPdfService(ILogger<TrendPdfService> logger, IDialogService DialogService)
{
    public async Task<Document> CreatePDF(Dictionary<DataPointCategory, IReadOnlyDictionary<int, IReadOnlyList<DataPoint>>> points)
    {
        QuestPDF.Settings.License = LicenseType.Community;
        //var title = $"{date.Year}-{date.Month:00}";
        await Task.CompletedTask;
        return Document.Create((dc) => dc.Page((pd) => pd.Content()));
    }

    [SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "All platforms are supported or not relevant")]
    public async Task SavePDF(Document document)
    {
        await using var stream = new MemoryStream(document.GeneratePdf());
        var saveResult = await FileSaver.Default.SaveAsync($"JournalApp {document.GetMetadata().Title}.pdf", stream, CancellationToken.None);

        if (!saveResult.IsSuccessful)
        {
            logger.LogInformation($"Couldn't save: {saveResult}");
            await DialogService.ShowMessageBox(string.Empty, "Was unable to save the file.");
            return;
        }

        logger.LogInformation($"Saved: {saveResult}");
    }
}
