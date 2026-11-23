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
    };

    /// <summary>
    /// The format this app writes. Only raise it for a change that an older reader would get wrong.
    /// </summary>
    public const int CurrentFormatVersion = 1;

    /// <summary>
    /// The format the backup was written in, or null for backups from before this was recorded.
    /// </summary>
    public int? FormatVersion { get; set; }

    /// <summary>
    /// The version of the app that wrote the backup, for information only.
    /// </summary>
    public string AppVersion { get; set; }

    public IReadOnlyCollection<Day> Days { get; set; }

    public IReadOnlyCollection<DataPointCategory> Categories { get; set; }

    public IReadOnlyCollection<DataPoint> Points { get; set; }

    public IReadOnlyCollection<PreferenceBackup> PreferenceBackups { get; set; }

    /// <summary>
    /// Whether the backup was written by a newer app whose format this one may not fully understand.
    /// </summary>
    [JsonIgnore]
    public bool IsFromNewerApp => FormatVersion > CurrentFormatVersion;

    /// <summary>
    /// Reads a backup file from the specified stream.
    /// </summary>
    /// <exception cref="BackupReadException">The stream isn't a backup or the backup is damaged.</exception>
    public static async Task<BackupFile> ReadArchive(Stream stream)
    {
        ZipArchive archive;
        try
        {
            archive = await ZipArchive.CreateAsync(stream, ZipArchiveMode.Read, leaveOpen: true, entryNameEncoding: null);
        }
        catch (InvalidDataException ex)
        {
            throw new BackupReadException("This file isn't a Journal App backup, or it's damaged.", ex);
        }

        await using (archive)
        {
            // Match on the file name alone so a backup that was unzipped and zipped again inside a folder still imports.
            var entry = archive.Entries.FirstOrDefault(x => string.Equals(x.Name, InternalBackupFileName, StringComparison.OrdinalIgnoreCase))
                ?? throw new BackupReadException("This file isn't a Journal App backup.");

            BackupFile backup;
            try
            {
                await using var entryStream = await entry.OpenAsync();
                backup = await JsonSerializer.DeserializeAsync<BackupFile>(entryStream, SerializerOptions);
            }
            catch (Exception ex) when (ex is JsonException or InvalidDataException)
            {
                // InvalidDataException covers a failed checksum, which means the file changed after it was written.
                throw new BackupReadException("This backup is damaged and can't be read.", ex);
            }

            if (backup == null)
                throw new BackupReadException("The backup is empty.");

            // Sections can be missing from older or hand-edited backups, and everything downstream expects them to exist.
            backup.Days ??= [];
            backup.Categories ??= [];
            backup.Points ??= [];
            backup.PreferenceBackups ??= [];

            return backup;
        }
    }

    /// <summary>
    /// Reads a backup file from the specified path.
    /// </summary>
    public static async Task<BackupFile> ReadArchive(string path)
    {
        // Read-only access so a read-only file, or one that's open elsewhere, can still be imported.
        await using var fs = File.OpenRead(path);
        return await ReadArchive(fs);
    }

    /// <summary>
    /// Writes the backup file to the specified stream.
    /// </summary>
    public async Task WriteArchive(Stream stream)
    {
        await using var archive = await ZipArchive.CreateAsync(stream, ZipArchiveMode.Create, leaveOpen: true, entryNameEncoding: null);

        var entry = archive.CreateEntry(InternalBackupFileName);
        await using var entryStream = await entry.OpenAsync();

        await JsonSerializer.SerializeAsync(entryStream, this, SerializerOptions);
    }

    /// <summary>
    /// Writes the backup file to the specified path.
    /// </summary>
    public async Task WriteArchive(string path)
    {
        await using var stream = File.Create(path);
        await WriteArchive(stream);
    }
}

/// <summary>
/// Thrown when a file can't be read as a backup. The message is written for the user.
/// </summary>
public sealed class BackupReadException(string message, Exception innerException = null) : Exception(message, innerException);

/// <summary>
/// Represents a preference.
/// </summary>
public record class PreferenceBackup(string Name, string Value);
