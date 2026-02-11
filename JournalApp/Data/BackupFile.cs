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
    public static async Task<BackupFile> ReadArchive(Stream stream)
    {
        await using var archive = await ZipArchive.CreateAsync(stream, ZipArchiveMode.Read, leaveOpen: true, entryNameEncoding: null);

        foreach (var entry in archive.Entries)
        {
            if (entry.FullName == InternalBackupFileName)
            {
                await using var entryStream = await entry.OpenAsync();

                return await JsonSerializer.DeserializeAsync<BackupFile>(entryStream, SerializerOptions);
            }
        }

        throw new InvalidOperationException("No valid backup found!");
    }

    /// <summary>
    /// Reads a backup file from the specified path.
    /// </summary>
    public static async Task<BackupFile> ReadArchive(string path)
    {
        await using var fs = File.Open(path, FileMode.Open);
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
/// Represents a preference.
/// </summary>
public record class PreferenceBackup(string Name, string Value);
