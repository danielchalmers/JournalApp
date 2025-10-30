using CommunityToolkit.Maui.Storage;

namespace JournalApp;

/// <summary>
/// Handles the export wizard workflow for backing up user data.
/// </summary>
public sealed class ExportWizard(
    ILogger<ExportWizard> logger,
    BackupCreator backupCreator,
    IFileSaver fileSaver,
    PreferenceService preferenceService)
{
    /// <summary>
    /// Executes the export wizard workflow.
    /// </summary>
    /// <param name="onProgress">Optional callback for progress updates</param>
    [SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "All platforms are supported")]
    public async Task<ExportResult> ExecuteAsync(Action<string>? onProgress = null, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Starting export wizard");
        var totalSw = Stopwatch.StartNew();

        try
        {
            // Step 1: Create backup data in memory
            onProgress?.Invoke("Creating backup...");
            logger.LogDebug("Creating backup data");
            
            var backupSw = Stopwatch.StartNew();
            using var memoryStream = new MemoryStream();
            var backupResult = await backupCreator.CreateBackupArchiveAsync(memoryStream, cancellationToken);

            if (!backupResult.IsSuccessful)
            {
                logger.LogWarning("Backup creation failed");
                return ExportResult.Failure(backupResult.Exception!);
            }

            logger.LogDebug("Backup created in memory in {ElapsedMilliseconds}ms", backupSw.ElapsedMilliseconds);

            // Step 2: Prompt user to select save location with the backup ready
            onProgress?.Invoke("Choose save location...");
            var fileName = $"backup-{DateTime.Now:yyyy-MM-dd}.journalapp";
            memoryStream.Position = 0; // Reset stream position for reading
            
            logger.LogDebug("Prompting user to select save location");
            var fileSaveResult = await fileSaver.SaveAsync(fileName, memoryStream, cancellationToken);

            if (!fileSaveResult.IsSuccessful)
            {
                if (fileSaveResult.Exception != null)
                {
                    logger.LogWarning(fileSaveResult.Exception, "File save failed");
                    return ExportResult.Failure(fileSaveResult.Exception);
                }
                else
                {
                    logger.LogInformation("File save cancelled by user");
                    return ExportResult.Cancelled();
                }
            }

            logger.LogDebug("Backup saved to {FilePath}", fileSaveResult.FilePath);

            // Update last export date
            preferenceService.LastExportDate = DateTimeOffset.Now;

            totalSw.Stop();
            logger.LogInformation("Export completed successfully in {ElapsedMilliseconds}ms", totalSw.ElapsedMilliseconds);

            return ExportResult.Success(fileSaveResult.FilePath);
        }
        catch (Exception ex)
        {
            totalSw.Stop();
            logger.LogError(ex, "Export wizard failed after {ElapsedMilliseconds}ms", totalSw.ElapsedMilliseconds);
            return ExportResult.Failure(ex);
        }
    }
}

/// <summary>
/// Result of an export operation.
/// </summary>
public sealed class ExportResult
{
    public bool IsSuccessful { get; init; }
    public bool WasCancelled { get; init; }
    public string? FilePath { get; init; }
    public Exception? Exception { get; init; }

    public static ExportResult Success(string filePath) => 
        new() { IsSuccessful = true, FilePath = filePath };
    
    public static ExportResult Cancelled() => 
        new() { WasCancelled = true };
    
    public static ExportResult Failure(Exception ex) => 
        new() { Exception = ex };
}
