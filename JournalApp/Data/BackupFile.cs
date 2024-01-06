using System.Globalization;
using System.IO.Compression;
using CsvHelper;

namespace JournalApp;

public class BackupFile
{
    public IEnumerable<Day> Days { get; set; }

    public IEnumerable<DataPointCategory> Categories { get; set; }

    public IEnumerable<DataPoint> Points { get; set; }

    public IEnumerable<BackupPreference> BackupPreferences { get; set; }

    public void WriteArchive(MemoryStream ms)
    {
        using var archive = new ZipArchive(ms, ZipArchiveMode.Create, true);

        WriteCsvToArchive(archive, "days.csv", Days);
        WriteCsvToArchive(archive, "categories.csv", Categories);
        WriteCsvToArchive(archive, "points.csv", Points);

        var backupPreferences = new List<BackupPreference>();
        foreach (var key in new[] { "safety_plan", "mood_palette", "last_export" })
            backupPreferences.Add(new(key, Preferences.Get(key, string.Empty)));

        WriteCsvToArchive(archive, "preferences.csv", backupPreferences);
    }

    private void WriteCsvToArchive<T>(ZipArchive archive, string fileName, IEnumerable<T> records)
    {
        if (!records.Any())
            return;

        var orderEntry = archive.CreateEntry(fileName);

        using var writer = new StreamWriter(orderEntry.Open());
        using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);

        csv.WriteRecords(records);
    }

    public static BackupFile ReadArchive(Stream stream)
    {
        var backupFile = new BackupFile();

        using var archive = new ZipArchive(stream, ZipArchiveMode.Read, true);

        foreach (var entry in archive.Entries)
        {
            switch (entry.FullName)
            {
                case "days.csv":
                    backupFile.Days = ReadCsvFromArchive<Day>(entry);
                    break;

                case "categories.csv":
                    backupFile.Categories = ReadCsvFromArchive<DataPointCategory>(entry);
                    break;

                case "points.csv":
                    backupFile.Points = ReadCsvFromArchive<DataPoint>(entry);
                    break;

                case "preferences.csv":
                    backupFile.BackupPreferences = ReadCsvFromArchive<BackupPreference>(entry);
                    break;
            }
        }

        return backupFile;
    }

    private static List<T> ReadCsvFromArchive<T>(ZipArchiveEntry entry)
    {
        using var entryStream = entry.Open();
        using var reader = new StreamReader(entryStream);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

        return csv.GetRecords<T>().ToList();
    }
}

public record class BackupPreference(string Name, string Value);