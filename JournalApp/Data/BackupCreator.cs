namespace JournalApp;

/// <summary>
/// Handles the creation of backup archives.
/// </summary>
public sealed class BackupCreator(ILogger<BackupCreator> logger, AppDataService appDataService)
{
    /// <summary>
    /// Creates a backup file and writes it to the provided stream.
    /// </summary>
    public async Task<BackupCreationResult> CreateBackupArchiveAsync(Stream targetStream, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Starting backup creation");
        var sw = Stopwatch.StartNew();

        try
        {
            // Create backup data from database
            var backupFile = await appDataService.CreateBackup();
            logger.LogDebug("Backup data constructed in {ElapsedMilliseconds}ms", sw.ElapsedMilliseconds);

            // Write archive to stream
            sw.Restart();
            await backupFile.WriteArchive(targetStream);
            logger.LogDebug("Archive written in {ElapsedMilliseconds}ms", sw.ElapsedMilliseconds);

            return BackupCreationResult.Success();
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to create backup archive after {ElapsedMilliseconds}ms", sw.ElapsedMilliseconds);
            return BackupCreationResult.Failure(ex);
        }
    }
}

/// <summary>
/// Result of a backup creation operation.
/// </summary>
public sealed class BackupCreationResult
{
    public bool IsSuccessful { get; init; }
    public Exception? Exception { get; init; }

    public static BackupCreationResult Success() => new() { IsSuccessful = true };
    public static BackupCreationResult Failure(Exception ex) => new() { IsSuccessful = false, Exception = ex };
}
