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
    public void PrepareDatabase_AppliesRemainingMigrations_WhenDatabaseIsBehind()
    {
        using var db = Services.GetService<IDbContextFactory<AppDbContext>>().CreateDbContext();
        var migrations = db.Database.GetMigrations().ToList();
        db.GetService<IMigrator>().Migrate(migrations[0]);
        db.Database.GetAppliedMigrations().Should().Equal(migrations[0]);

        Services.GetService<AppDbSeeder>().PrepareDatabase();

        db.Database.GetAppliedMigrations().Should().Equal(migrations);
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
