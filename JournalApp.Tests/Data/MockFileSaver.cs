using CommunityToolkit.Maui.Storage;

namespace JournalApp.Tests.Data;

public class MockFileSaver : IFileSaver
{
    public Task<FileSaverResult> SaveAsync(string fileName, Stream stream, CancellationToken cancellationToken = default)
    {
        // Return a successful result for testing
        return Task.FromResult(new FileSaverResult(Path.Combine(Path.GetTempPath(), fileName), null));
    }

    public Task<FileSaverResult> SaveAsync(string fileName, Stream stream, IProgress<double> progress, CancellationToken cancellationToken = default)
    {
        // Return a successful result for testing
        return Task.FromResult(new FileSaverResult(Path.Combine(Path.GetTempPath(), fileName), null));
    }

    public Task<FileSaverResult> SaveAsync(string initialPath, string fileName, Stream stream, CancellationToken cancellationToken = default)
    {
        // Return a successful result for testing
        return Task.FromResult(new FileSaverResult(Path.Combine(initialPath, fileName), null));
    }

    public Task<FileSaverResult> SaveAsync(string initialPath, string fileName, Stream stream, IProgress<double> progress, CancellationToken cancellationToken = default)
    {
        // Return a successful result for testing
        return Task.FromResult(new FileSaverResult(Path.Combine(initialPath, fileName), null));
    }
}
