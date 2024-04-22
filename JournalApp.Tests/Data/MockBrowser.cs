namespace JournalApp.Tests.Data;

public class MockBrowser : IBrowser
{
    public Task<bool> OpenAsync(Uri uri, BrowserLaunchOptions options) => throw new NotImplementedException();
}
