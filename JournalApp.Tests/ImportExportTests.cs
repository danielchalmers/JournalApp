namespace JournalApp.Tests;

public class ImportExportTests : JaTestContext
{
    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();

        AddDbContext();
    }

    [Fact(Skip = "Stub")]
    public async Task EverythingInBackupIsImported() { }

    [Fact(Skip = "Stub")]
    public async Task ExportFormatHasNotChanged() { }
}
