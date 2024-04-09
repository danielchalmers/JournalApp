using System.IO.Compression;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace JournalApp;

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

    public static async Task<BackupFile> ReadArchive(Stream stream)
    {
        using var archive = new ZipArchive(stream, ZipArchiveMode.Read);

        foreach (var entry in archive.Entries)
        {
            if (entry.FullName == InternalBackupFileName)
            {
                await using var entryStream = entry.Open();

                return await JsonSerializer.DeserializeAsync<BackupFile>(entryStream, SerializerOptions);
            }
        }

        throw new InvalidOperationException("No valid backup found!");
    }

    public static async Task<BackupFile> ReadArchive(string path)
    {
        await using var fs = File.Open(path, FileMode.Open);
        return await ReadArchive(fs);
    }

    public async Task WriteArchive(Stream stream)
    {
        using var archive = new ZipArchive(stream, ZipArchiveMode.Create);

        var entry = archive.CreateEntry(InternalBackupFileName);
        await using var entryStream = entry.Open();

        await JsonSerializer.SerializeAsync(entryStream, this, SerializerOptions);
    }

    public async Task WriteArchive(string path)
    {
        await using var stream = File.Create(path);
        await WriteArchive(stream);
    }
}

public record class PreferenceBackup(string Name, string Value);
