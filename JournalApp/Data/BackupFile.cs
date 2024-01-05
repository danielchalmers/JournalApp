using System.Globalization;
using System.IO.Compression;
using CsvHelper;

namespace JournalApp;

public class BackupFile
{
    public IEnumerable<Day> Days { get; set; }

    public IEnumerable<DataPointCategory> Categories { get; set; }

    public IEnumerable<DataPoint> Points { get; set; }

    public void WriteArchive(MemoryStream ms)
    {
        using var archive = new ZipArchive(ms, ZipArchiveMode.Create, true);

        WriteCsvFileToArchive(archive, "days.csv", Days);
        WriteCsvFileToArchive(archive, "categories.csv", Categories);
        WriteCsvFileToArchive(archive, "points.csv", Points);
    }

    private void WriteCsvFileToArchive<T>(ZipArchive archive, string fileName, IEnumerable<T> records)
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
            if (entry.FullName == "days.csv")
                backupFile.Days = ReadCsvFileFromArchive<Day>(entry);

            if (entry.FullName == "categories.csv")
                backupFile.Categories = ReadCsvFileFromArchive<DataPointCategory>(entry);

            if (entry.FullName == "points.csv")
                backupFile.Points = ReadCsvFileFromArchive<DataPoint>(entry);

            // TODO: Back up preferences.
        }

        return backupFile;
    }

    private static List<T> ReadCsvFileFromArchive<T>(ZipArchiveEntry entry)
    {
        using var entryStream = entry.Open();
        using var reader = new StreamReader(entryStream);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

        return csv.GetRecords<T>().ToList();
    }
}
