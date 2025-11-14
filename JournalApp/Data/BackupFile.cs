using System.IO.Compression;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace JournalApp;

/// <summary>
/// Handles backup and restore operations for the journal data.
/// </summary>
public class BackupFile
{
    private const string InternalBackupFileName = "journalapp-data.json";

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        ReferenceHandler = ReferenceHandler.Preserve,
        WriteIndented = true,
    };

    public IReadOnlyCollection<Day> Days { get; set; }

    public IReadOnlyCollection<DataPointCategory> Categories { get; set; }

    public IReadOnlyCollection<DataPoint> Points { get; set; }

    public IReadOnlyCollection<PreferenceBackup> PreferenceBackups { get; set; }

    /// <summary>
    /// Reads a backup file from the specified stream.
    /// </summary>
    public static async Task<BackupFile> ReadArchive(Stream stream, CancellationToken cancellationToken = default)
    {
        await using var archive = await ZipArchive.CreateAsync(stream, ZipArchiveMode.Read, leaveOpen: false, entryNameEncoding: null, cancellationToken).ConfigureAwait(false);

        foreach (var entry in archive.Entries)
        {
            if (entry.FullName == InternalBackupFileName)
            {
                await using var entryStream = await entry.OpenAsync(cancellationToken).ConfigureAwait(false);

                return await JsonSerializer.DeserializeAsync<BackupFile>(entryStream, SerializerOptions, cancellationToken).ConfigureAwait(false);
            }
        }

        throw new InvalidOperationException("No valid backup found!");
    }

    /// <summary>
    /// Reads a backup file from the specified path.
    /// </summary>
    public static async Task<BackupFile> ReadArchive(string path, CancellationToken cancellationToken = default)
    {
        await using var fs = File.Open(path, FileMode.Open);
        return await ReadArchive(fs, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Writes the backup file to the specified stream.
    /// </summary>
    public async Task WriteArchive(Stream stream, CancellationToken cancellationToken = default)
    {
        await using var archive = await ZipArchive.CreateAsync(stream, ZipArchiveMode.Create, leaveOpen: false, entryNameEncoding: null, cancellationToken).ConfigureAwait(false);

        var entry = archive.CreateEntry(InternalBackupFileName);
        await using var entryStream = await entry.OpenAsync(cancellationToken).ConfigureAwait(false);

        await JsonSerializer.SerializeAsync(entryStream, this, SerializerOptions, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Writes the backup file to the specified path.
    /// </summary>
    public async Task WriteArchive(string path, CancellationToken cancellationToken = default)
    {
        await using var stream = File.Create(path);
        await WriteArchive(stream, cancellationToken).ConfigureAwait(false);
    }
}

/// <summary>
/// Represents a preference.
/// </summary>
public record class PreferenceBackup(string Name, string Value);
