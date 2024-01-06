using System.IO.Compression;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace JournalApp;

public class BackupFile
{
    private const string InternalBackupFileName = "journalapp-backup.json";

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        ReferenceHandler = ReferenceHandler.Preserve,
        WriteIndented = true,
    };

    public IEnumerable<Day> Days { get; set; }

    public IEnumerable<DataPointCategory> Categories { get; set; }

    public IEnumerable<DataPoint> Points { get; set; }

    public IEnumerable<PreferenceBackup> PreferenceBackups { get; set; }

    public async Task WriteArchive(MemoryStream ms)
    {
        using var archive = new ZipArchive(ms, ZipArchiveMode.Create, true);
        var entry = archive.CreateEntry(InternalBackupFileName);
        await using var entryStream = entry.Open();

        await JsonSerializer.SerializeAsync(entryStream, this, SerializerOptions);
    }

    public static async Task<BackupFile> ReadArchive(Stream stream)
    {
        using var archive = new ZipArchive(stream, ZipArchiveMode.Read, true);

        foreach (var entry in archive.Entries)
        {
            if (entry.FullName == InternalBackupFileName)
            {
                await using var entryStream = entry.Open();
                return await JsonSerializer.DeserializeAsync<BackupFile>(entryStream, SerializerOptions);
            }
        }

        throw new InvalidOperationException("No backup file found!");
    }
}

public record class PreferenceBackup(string Name, string Value);