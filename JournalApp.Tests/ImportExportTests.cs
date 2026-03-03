using Microsoft.EntityFrameworkCore;

namespace JournalApp.Tests;

public class ImportExportTests : JaTestContext
{
    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();

        AddDbContext();
    }

    [Fact]
    public async Task EverythingInBackupIsImportedAndFormatHasNotChanged()
    {
        var backup = await BackupFile.ReadArchive("Data/backup-2023-01-01-to-2023-01-08.journalapp");

        var appDataService = Services.GetService<AppDataService>();
        await appDataService.RestoreDbSets(backup);
        AssertBackupMatchesDatabase(backup);
    }

    [Fact]
    public async Task ExportThenImport()
    {
        var dbf = Services.GetService<IDbContextFactory<AppDbContext>>();
        var appDataService = Services.GetService<AppDataService>();
        var appDbSeeder = Services.GetService<AppDbSeeder>();

        var dates = new DateOnly(2023, 01, 01).DatesTo(new(2023, 01, 08));
        appDbSeeder.SeedCategories();
        appDbSeeder.SeedDays(dates);

        // Assert that a backup has the same information as the database.

        var backup = await appDataService.CreateBackup();

        AssertBackupMatchesDatabase(backup);

        // Assert that database clears.

        await appDataService.DeleteDbSets();

        using (var db = dbf.CreateDbContext())
        {
            db.Days.Should().BeEmpty();
            db.Categories.Should().BeEmpty();
            db.Points.Should().BeEmpty();
        }

        // Assert that the database was restored from the backup.

        await appDataService.RestoreDbSets(backup);

        AssertBackupMatchesDatabase(backup);
    }

    [Fact]
    public async Task ReplaceDbSetsIsAtomic()
    {
        var dbf = Services.GetService<IDbContextFactory<AppDbContext>>();
        var appDataService = Services.GetService<AppDataService>();
        var appDbSeeder = Services.GetService<AppDbSeeder>();

        // Seed initial data
        var dates = new DateOnly(2023, 01, 01).DatesTo(new(2023, 01, 08));
        appDbSeeder.SeedCategories();
        appDbSeeder.SeedDays(dates);

        var initialBackup = await appDataService.CreateBackup();
        var initialDayGuids = initialBackup.Days.Select(x => x.Guid).ToList();

        // Create a new backup with different data
        await appDataService.DeleteDbSets();
        appDbSeeder.SeedCategories();
        var dates2 = new DateOnly(2024, 01, 01).DatesTo(new(2024, 01, 03));
        appDbSeeder.SeedDays(dates2);
        var newBackup = await appDataService.CreateBackup();

        // Replace atomically - should delete old data and restore new data in a single transaction
        await appDataService.ReplaceDbSets(newBackup);

        // Verify the new data is present
        AssertBackupMatchesDatabase(newBackup);

        // Verify the old data is gone
        using (var db = dbf.CreateDbContext())
        {
            db.Days.Select(x => x.Guid).Should().NotContain(initialDayGuids);
        }
    }

    [Fact]
    public async Task ReadArchive_ThrowsOnInvalidZip()
    {
        // Arrange
        var invalidZipPath = Path.Combine(Path.GetTempPath(), $"invalid-{Guid.NewGuid()}.journalapp");
        await File.WriteAllTextAsync(invalidZipPath, "This is not a valid zip file");

        try
        {
            // Act & Assert
            var act = async () => await BackupFile.ReadArchive(invalidZipPath);
            await act.Should().ThrowAsync<InvalidDataException>();
        }
        finally
        {
            if (File.Exists(invalidZipPath))
                File.Delete(invalidZipPath);
        }
    }

    [Fact]
    public async Task ReadArchive_ThrowsOnMissingInternalFile()
    {
        // Arrange - Create a valid ZIP without the required internal file
        var zipPath = Path.Combine(Path.GetTempPath(), $"empty-{Guid.NewGuid()}.journalapp");

        try
        {
            using (var stream = File.Create(zipPath))
            using (var archive = new System.IO.Compression.ZipArchive(stream, System.IO.Compression.ZipArchiveMode.Create))
            {
                // Create an entry with wrong name
                var entry = archive.CreateEntry("wrong-name.json");
                using var entryStream = entry.Open();
                await entryStream.WriteAsync(System.Text.Encoding.UTF8.GetBytes("{}"));
            }

            // Act & Assert
            var act = async () => await BackupFile.ReadArchive(zipPath);
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("No valid backup found!*");
        }
        finally
        {
            if (File.Exists(zipPath))
                File.Delete(zipPath);
        }
    }

    [Fact]
    public async Task ReadArchive_ThrowsOnCorruptedJSON()
    {
        // Arrange - Create a valid ZIP with corrupted JSON
        var zipPath = Path.Combine(Path.GetTempPath(), $"corrupted-{Guid.NewGuid()}.journalapp");

        try
        {
            using (var stream = File.Create(zipPath))
            using (var archive = new System.IO.Compression.ZipArchive(stream, System.IO.Compression.ZipArchiveMode.Create))
            {
                var entry = archive.CreateEntry("journalapp-data.json");
                using var entryStream = entry.Open();
                await entryStream.WriteAsync(System.Text.Encoding.UTF8.GetBytes("{invalid json content"));
            }

            // Act & Assert
            var act = async () => await BackupFile.ReadArchive(zipPath);
            await act.Should().ThrowAsync<System.Text.Json.JsonException>();
        }
        finally
        {
            if (File.Exists(zipPath))
                File.Delete(zipPath);
        }
    }

    [Fact]
    public async Task ReadArchive_HandlesEmptyBackupFile()
    {
        // Arrange - Create a valid ZIP with valid but empty JSON
        var zipPath = Path.Combine(Path.GetTempPath(), $"empty-backup-{Guid.NewGuid()}.journalapp");

        try
        {
            var emptyBackup = new BackupFile
            {
                Days = [],
                Categories = [],
                Points = [],
                PreferenceBackups = []
            };

            await emptyBackup.WriteArchive(zipPath);

            // Act
            var restoredBackup = await BackupFile.ReadArchive(zipPath);

            // Assert
            restoredBackup.Should().NotBeNull();
            restoredBackup.Days.Should().BeEmpty();
            restoredBackup.Categories.Should().BeEmpty();
            restoredBackup.Points.Should().BeEmpty();
        }
        finally
        {
            if (File.Exists(zipPath))
                File.Delete(zipPath);
        }
    }

    [Fact]
    public async Task WriteArchive_CreatesValidZipFile()
    {
        // Arrange
        var appDbSeeder = Services.GetService<AppDbSeeder>();
        var appDataService = Services.GetService<AppDataService>();

        appDbSeeder.SeedCategories();
        var dates = new DateOnly(2024, 1, 1).DatesTo(new(2024, 1, 3));
        appDbSeeder.SeedDays(dates);

        var backup = await appDataService.CreateBackup();
        var zipPath = Path.Combine(Path.GetTempPath(), $"test-backup-{Guid.NewGuid()}.journalapp");

        try
        {
            // Act
            await backup.WriteArchive(zipPath);

            // Assert
            File.Exists(zipPath).Should().BeTrue();

            // Verify it can be read back
            var restoredBackup = await BackupFile.ReadArchive(zipPath);
            restoredBackup.Days.Select(d => d.Guid).Should().BeEquivalentTo(backup.Days.Select(d => d.Guid));
            restoredBackup.Categories.Select(c => c.Guid).Should().BeEquivalentTo(backup.Categories.Select(c => c.Guid));
            restoredBackup.Points.Select(p => p.Guid).Should().BeEquivalentTo(backup.Points.Select(p => p.Guid));
        }
        finally
        {
            if (File.Exists(zipPath))
                File.Delete(zipPath);
        }
    }

    [Fact]
    public async Task RoundTrip_PreservesAllDataProperties()
    {
        // Arrange
        var dbFactory = Services.GetService<IDbContextFactory<AppDbContext>>();
        var appDbSeeder = Services.GetService<AppDbSeeder>();
        var appDataService = Services.GetService<AppDataService>();

        appDbSeeder.SeedCategories();

        // Create data with various properties set
        using (var db = await dbFactory.CreateDbContextAsync())
        {
            var day = Day.Create(new DateOnly(2024, 1, 1));
            db.Days.Add(day);

            // Add a mood point
            var moodCategory = db.Categories.First(c => c.Type == PointType.Mood);
            var moodPoint = DataPoint.Create(day, moodCategory);
            moodPoint.Mood = "😀";
            moodPoint.Text = "Test note with special chars: é, ñ, 中文";
            db.Points.Add(moodPoint);

            // Add a medication point
            var medCategory = db.Categories.First(c => c.Type == PointType.Medication);
            var medPoint = DataPoint.Create(day, medCategory);
            medPoint.Bool = true;
            medPoint.MedicationDose = 150.5m;
            db.Points.Add(medPoint);

            // Add a LowToHigh point (seeded categories use this instead of Scale)
            var scaleCategory = db.Categories.First(c => c.Type == PointType.LowToHigh);
            var scalePoint = DataPoint.Create(day, scaleCategory);
            scalePoint.ScaleIndex = 3;
            db.Points.Add(scalePoint);

            await db.SaveChangesAsync();
        }

        var zipPath = Path.Combine(Path.GetTempPath(), $"roundtrip-{Guid.NewGuid()}.journalapp");

        try
        {
            // Act - Export to file
            var originalBackup = await appDataService.CreateBackup();
            await originalBackup.WriteArchive(zipPath);

            // Clear database
            await appDataService.DeleteDbSets();

            // Import from file
            var restoredBackup = await BackupFile.ReadArchive(zipPath);
            await appDataService.RestoreDbSets(restoredBackup);

            // Assert - Verify all properties are preserved
            using (var db = await dbFactory.CreateDbContextAsync())
            {
                var moodPoint = db.Points.Include(p => p.Category).First(p => p.Category.Type == PointType.Mood);
                moodPoint.Mood.Should().Be("😀");
                moodPoint.Text.Should().Be("Test note with special chars: é, ñ, 中文");

                var medPoint = db.Points.Include(p => p.Category).First(p => p.Category.Type == PointType.Medication);
                medPoint.Bool.Should().BeTrue();
                medPoint.MedicationDose.Should().Be(150.5m);

                var scalePoint = db.Points.Include(p => p.Category).First(p => p.Category.Type == PointType.LowToHigh);
                scalePoint.ScaleIndex.Should().Be(3);
            }
        }
        finally
        {
            if (File.Exists(zipPath))
                File.Delete(zipPath);
        }
    }

    [Fact]
    public async Task RestoreDbSets_HandlesOrphanedDataPoints()
    {
        // Points with null day/category references should still round-trip in backup restore
        // because these FKs are nullable in the current schema.
        var appDataService = Services.GetService<AppDataService>();
        var dbFactory = Services.GetService<IDbContextFactory<AppDbContext>>();

        var orphanedPoint = new DataPoint
        {
            Guid = Guid.NewGuid(),
            Day = null!,
            Category = null!,
            Type = PointType.Bool,
            CreatedAt = DateTimeOffset.Now
        };

        var backup = new BackupFile
        {
            Days = [],
            Categories = [],
            Points = [orphanedPoint],
            PreferenceBackups = []
        };

        // Act
        await appDataService.RestoreDbSets(backup);

        // Assert
        using (var db = await dbFactory.CreateDbContextAsync())
        {
            db.Days.Should().BeEmpty();
            db.Categories.Should().BeEmpty();
            db.Points.Should().HaveCount(1);

            var restoredPoint = db.Points.Include(p => p.Day).Include(p => p.Category).Single();
            restoredPoint.Guid.Should().Be(orphanedPoint.Guid);
            restoredPoint.Day.Should().BeNull();
            restoredPoint.Category.Should().BeNull();
        }
    }

    private void AssertBackupMatchesDatabase(BackupFile backup)
    {
        var dbf = Services.GetService<IDbContextFactory<AppDbContext>>();

        using var db = dbf.CreateDbContext();

        var backupDays = backup.Days.ToList();
        var backupCategories = backup.Categories.ToList();
        var backupPoints = backup.Points.ToList();

        var dbDays = db.Days.Include(x => x.Points).AsNoTracking().ToList();
        var dbCategories = db.Categories.Include(x => x.Points).AsNoTracking().ToList();
        var dbPoints = db.Points.Include(x => x.Day).Include(x => x.Category).AsNoTracking().ToList();

        var backupDayByPoint = backupDays
            .SelectMany(day => day.Points.Select(point => new { point.Guid, DayGuid = day.Guid }))
            .ToList();
        backupDayByPoint.Select(x => x.Guid).Should().OnlyHaveUniqueItems();
        var backupDayGuidByPoint = backupDayByPoint.ToDictionary(x => x.Guid, x => x.DayGuid);
        backupDayGuidByPoint.Keys.Should().BeEquivalentTo(backupPoints.Select(x => x.Guid));

        var backupCategoryByPoint = backupCategories
            .SelectMany(category => category.Points.Select(point => new { point.Guid, CategoryGuid = category.Guid }))
            .ToList();
        backupCategoryByPoint.Select(x => x.Guid).Should().OnlyHaveUniqueItems();
        var backupCategoryGuidByPoint = backupCategoryByPoint.ToDictionary(x => x.Guid, x => x.CategoryGuid);
        backupCategoryGuidByPoint.Keys.Should().BeEquivalentTo(backupPoints.Select(x => x.Guid));

        backupDays.Select(x => new
        {
            x.Guid,
            x.Date,
            PointGuids = x.Points.Select(p => p.Guid).OrderBy(g => g).ToList(),
        }).Should().NotBeEmpty().And.BeEquivalentTo(dbDays.Select(x => new
        {
            x.Guid,
            x.Date,
            PointGuids = x.Points.Select(p => p.Guid).OrderBy(g => g).ToList(),
        }));

        backupCategories.Select(x => new
        {
            x.Guid,
            x.Group,
            x.Name,
            x.Index,
            x.ReadOnly,
            x.Enabled,
            x.Deleted,
            x.Type,
            x.MedicationDose,
            x.MedicationUnit,
            x.MedicationEveryDaySince,
            x.Details,
            PointGuids = x.Points.Select(p => p.Guid).OrderBy(g => g).ToList(),
        }).Should().NotBeEmpty().And.BeEquivalentTo(dbCategories.Select(x => new
        {
            x.Guid,
            x.Group,
            x.Name,
            x.Index,
            x.ReadOnly,
            x.Enabled,
            x.Deleted,
            x.Type,
            x.MedicationDose,
            x.MedicationUnit,
            x.MedicationEveryDaySince,
            x.Details,
            PointGuids = x.Points.Select(p => p.Guid).OrderBy(g => g).ToList(),
        }));

        backupPoints.Select(x => new
        {
            x.Guid,
            x.CreatedAt,
            x.Type,
            x.Deleted,
            x.Mood,
            x.SleepHours,
            x.ScaleIndex,
            x.Bool,
            x.Number,
            x.Text,
            x.MedicationDose,
            DayGuid = backupDayGuidByPoint[x.Guid],
            CategoryGuid = backupCategoryGuidByPoint[x.Guid],
        }).Should().NotBeEmpty().And.BeEquivalentTo(dbPoints.Select(x => new
        {
            x.Guid,
            x.CreatedAt,
            x.Type,
            x.Deleted,
            x.Mood,
            x.SleepHours,
            x.ScaleIndex,
            x.Bool,
            x.Number,
            x.Text,
            x.MedicationDose,
            DayGuid = x.Day.Guid,
            CategoryGuid = x.Category.Guid,
        }));
    }
}
