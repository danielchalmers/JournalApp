using JournalApp.CompiledModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace JournalApp.Tests.Data;

public class AppDbSeederTests : JaTestContext
{
    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        AddDbContext();
    }

    [Fact]
    public void CompiledModel_MatchesTheEntities()
    {
        using var db = Services.GetService<IDbContextFactory<AppDbContext>>().CreateDbContext();

        // The app skips building its model at launch by using the generated one, so that one must describe the entities as they are now.
        db.Model.Should().BeSameAs(AppDbContextModel.Instance);

        var compiled = db.Model.ToDebugString(MetadataDebugStringOptions.ShortDefault);
        var current = db.GetService<IDesignTimeModel>().Model.ToDebugString(MetadataDebugStringOptions.ShortDefault);

        compiled.Should().Be(current, "the compiled model is out of date; regenerate it with: dotnet ef dbcontext optimize --project JournalApp --framework net11.0 --output-dir Data/CompiledModels --namespace JournalApp.CompiledModels");
    }

    [Fact]
    public void DesignTimeFactory_ConfiguresTheContextLikeTheApp()
    {
        using var db = new AppDbContextDesignTimeFactory().CreateDbContext([]);

        // The tooling generates the compiled model from this context, and EF only accepts that model for matching options.
        db.Model.Should().BeSameAs(AppDbContextModel.Instance);
    }

    [Fact]
    public void PrepareDatabase_AppliesEveryMigration_ToNewDatabase()
    {
        using var db = Services.GetService<IDbContextFactory<AppDbContext>>().CreateDbContext();

        db.Database.GetAppliedMigrations().Should().Equal(db.Database.GetMigrations());
    }

    [Fact]
    public async Task PrepareDatabase_AppliesRemainingMigrations_AndKeepsData_WhenDatabaseIsBehind()
    {
        var dbFactory = Services.GetService<IDbContextFactory<AppDbContext>>();
        var appDbSeeder = Services.GetService<AppDbSeeder>();
        appDbSeeder.SeedCategories();
        int pointCount;

        using (var seedDb = await dbFactory.CreateDbContextAsync())
        {
            var day = await seedDb.GetOrCreateDayAndAddPoints(new DateOnly(2024, 1, 1));
            var note = seedDb.CreateNote(day);
            note.Text = "Written before the update";
            seedDb.Points.Add(note);
            await seedDb.SaveChangesAsync();
            pointCount = seedDb.Points.Count();
        }

        // Take the database back to how the first release left it, with the journal still in it.
        List<string> migrations;
        using (var oldDb = await dbFactory.CreateDbContextAsync())
        {
            migrations = oldDb.Database.GetMigrations().ToList();
            oldDb.GetService<IMigrator>().Migrate(migrations[0]);
            oldDb.Database.GetAppliedMigrations().Should().Equal(migrations[0]);
        }

        appDbSeeder.PrepareDatabase();

        using var db = await dbFactory.CreateDbContextAsync();
        db.Database.GetAppliedMigrations().Should().Equal(migrations);
        db.Days.Should().ContainSingle().Which.Date.Should().Be(new DateOnly(2024, 1, 1));
        db.Points.Count().Should().Be(pointCount);
        db.Points.Should().ContainSingle(p => p.Text == "Written before the update");
    }

    [Fact]
    public async Task PrepareDatabase_LeavesUpToDateDatabaseAlone()
    {
        var dbFactory = Services.GetService<IDbContextFactory<AppDbContext>>();
        var appDbSeeder = Services.GetService<AppDbSeeder>();
        appDbSeeder.SeedCategories();

        using (var seedDb = await dbFactory.CreateDbContextAsync())
        {
            await seedDb.GetOrCreateDayAndAddPoints(new DateOnly(2024, 1, 1));
            await seedDb.SaveChangesAsync();
        }

        appDbSeeder.PrepareDatabase();

        using var db = await dbFactory.CreateDbContextAsync();
        db.Database.GetAppliedMigrations().Should().Equal(db.Database.GetMigrations());
        db.Days.Should().ContainSingle();
        db.Points.Should().NotBeEmpty();
    }
}
