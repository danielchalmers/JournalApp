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
        AssertBackupIsRoughlyEqualToDatabase(backup);
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

        AssertBackupIsRoughlyEqualToDatabase(backup);

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

        AssertBackupIsRoughlyEqualToDatabase(backup);
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
        AssertBackupIsRoughlyEqualToDatabase(newBackup);

        // Verify the old data is gone
        using (var db = dbf.CreateDbContext())
        {
            db.Days.Select(x => x.Guid).Should().NotContain(initialDayGuids);
        }
    }

    private void AssertBackupIsRoughlyEqualToDatabase(BackupFile backup)
    {
        var dbf = Services.GetService<IDbContextFactory<AppDbContext>>();

        using var db = dbf.CreateDbContext();

        backup.Days.Select(x => x.Guid).Should().NotBeEmpty().And.BeEquivalentTo(db.Days.Select(x => x.Guid));
        backup.Categories.Select(x => x.Guid).Should().NotBeEmpty().And.BeEquivalentTo(db.Categories.Select(x => x.Guid));
        backup.Points.Select(x => x.Guid).Should().NotBeEmpty().And.BeEquivalentTo(db.Points.Select(x => x.Guid));

        // TODO: Test some actual data and linked relationships, not just GUIDs.
    }
}
