namespace JournalApp.Tests.Data;

public class MockShare : IShare
{
    public Task RequestAsync(ShareTextRequest request) => throw new NotImplementedException();
    public Task RequestAsync(ShareFileRequest request) => throw new NotImplementedException();
    public Task RequestAsync(ShareMultipleFilesRequest request) => throw new NotImplementedException();
}
