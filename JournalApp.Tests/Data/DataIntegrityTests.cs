using Microsoft.EntityFrameworkCore;

namespace JournalApp.Tests.Data;

/// <summary>
/// Tests for data integrity and edge cases in database operations that carry real app logic (index management, point generation, key/relationship invariants).
/// Plain "does EF persist a nullable column" round-trips live in the backup round-trip tests instead.
/// </summary>
public class DataIntegrityTests : JaTestContext
{
    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        AddDbContext();
    }

    [Fact]
    public async Task GetOrCreateDayAndAddPoints_HandlesMultipleConcurrentCalls()
    {
        // Test that multiple calls for the same date don't create duplicates

        // Arrange
        var dbFactory = Services.GetService<IDbContextFactory<AppDbContext>>();
        var appDbSeeder = Services.GetService<AppDbSeeder>();
        appDbSeeder.SeedCategories();

        var date = new DateOnly(2024, 1, 1);

        // Act - Call multiple times
        using (var db = await dbFactory.CreateDbContextAsync())
        {
            await db.GetOrCreateDayAndAddPoints(date);
            await db.SaveChangesAsync();
        }

        using (var db = await dbFactory.CreateDbContextAsync())
        {
            await db.GetOrCreateDayAndAddPoints(date);
            await db.SaveChangesAsync();
        }

        // Assert - Should only have one day
        using (var db = await dbFactory.CreateDbContextAsync())
        {
            db.Days.Count(d => d.Date == date).Should().Be(1);
        }
    }

    [Fact]
    public async Task Category_DuplicateGuidsNotAllowed()
    {
        // Arrange
        var dbFactory = Services.GetService<IDbContextFactory<AppDbContext>>();
        using var db = await dbFactory.CreateDbContextAsync();

        var guid = Guid.NewGuid();
        var category1 = new DataPointCategory
        {
            Guid = guid,
            Name = "Category 1",
            Group = "Test",
            Type = PointType.Bool
        };

        db.Categories.Add(category1);
        await db.SaveChangesAsync();

        // Create a new context to simulate a separate operation
        using var db2 = await dbFactory.CreateDbContextAsync();
        var category2 = new DataPointCategory
        {
            Guid = guid,  // Same GUID
            Name = "Category 2",
            Group = "Test",
            Type = PointType.Bool
        };

        db2.Categories.Add(category2);

        // Act & Assert - Should throw on duplicate GUID
        var act = async () => await db2.SaveChangesAsync();
        await act.Should().ThrowAsync<DbUpdateException>();
    }

    [Fact]
    public async Task Day_DuplicateDatesAllowed()
    {
        // Note: In practice, the app logic prevents duplicate dates, but the database schema doesn't enforce it (CalendarService relies on tolerating them).

        // Arrange
        var dbFactory = Services.GetService<IDbContextFactory<AppDbContext>>();
        using var db = await dbFactory.CreateDbContextAsync();

        var date = new DateOnly(2024, 1, 1);
        var day1 = Day.Create(date);
        var day2 = Day.Create(date);

        db.Days.Add(day1);
        db.Days.Add(day2);

        // Act & Assert - Should not throw (no unique constraint on date)
        await db.SaveChangesAsync();
        db.Days.Count(d => d.Date == date).Should().Be(2);
    }

    [Fact]
    public async Task DataPoint_PreservesExplicitGuidOnSave()
    {
        // Arrange
        var dbFactory = Services.GetService<IDbContextFactory<AppDbContext>>();
        var appDbSeeder = Services.GetService<AppDbSeeder>();
        appDbSeeder.SeedCategories();

        using var db = await dbFactory.CreateDbContextAsync();
        var day = Day.Create(new DateOnly(2024, 1, 1));
        db.Days.Add(day);

        var category = db.Categories.First();
        var point = DataPoint.Create(day, category);
        var explicitGuid = Guid.NewGuid();
        point.Guid = explicitGuid;

        db.Points.Add(point);
        await db.SaveChangesAsync();

        // Act - reload from a cleared change tracker
        db.ChangeTracker.Clear();
        var reloaded = await db.Points.SingleAsync(p => p.Guid == explicitGuid);

        // Assert - an explicitly assigned key is not regenerated
        reloaded.Guid.Should().Be(explicitGuid);
    }

    [Fact]
    public async Task GetMissingPoints_HandlesNullRandom()
    {
        // Arrange
        var dbFactory = Services.GetService<IDbContextFactory<AppDbContext>>();
        var appDbSeeder = Services.GetService<AppDbSeeder>();
        appDbSeeder.SeedCategories();

        using var db = await dbFactory.CreateDbContextAsync();
        var day = Day.Create(new DateOnly(2024, 1, 1));
        var category = db.Categories.First(c => c.Enabled && !c.Deleted && c.Group != "Notes");

        // Act - Call with null random (production mode)
        var points = db.GetMissingPoints(day, category, null);

        // Assert - Should create point without random data
        points.Should().HaveCount(1);
        var point = points.First();
        point.Mood.Should().BeNull();
        point.Number.Should().BeNull();
        point.Bool.Should().BeNull();
    }

    [Fact]
    public async Task GetMissingPoints_WithRandom_GeneratesRandomData()
    {
        // Arrange
        var dbFactory = Services.GetService<IDbContextFactory<AppDbContext>>();
        var appDbSeeder = Services.GetService<AppDbSeeder>();
        appDbSeeder.SeedCategories();

        using var db = await dbFactory.CreateDbContextAsync();
        var day = Day.Create(new DateOnly(2024, 1, 1));
        var category = db.Categories.First(c => c.Enabled && !c.Deleted && c.Group != "Notes");

        // Act - Call with random (debug mode)
        var random = new Random(42);
        var points = db.GetMissingPoints(day, category, random);

        // Assert - Should create point with random data (for appropriate types)
        points.Should().HaveCount(1);
        var point = points.First();

        // Random data should be populated based on category type
        if (category.Type == PointType.Mood)
        {
            point.Mood.Should().NotBeNull();
        }
        else if (category.Type == PointType.Number)
        {
            point.Number.Should().NotBeNull();
        }
    }

    [Fact]
    public async Task FixCategoryIndexes_HandlesEmptyGroup()
    {
        // Arrange
        var dbFactory = Services.GetService<IDbContextFactory<AppDbContext>>();
        using var db = await dbFactory.CreateDbContextAsync();

        // Act - Call on empty database
        db.FixCategoryIndexes();
        await db.SaveChangesAsync();

        // Assert - Should not throw
        db.Categories.Should().BeEmpty();
    }

    [Fact]
    public async Task MoveCategoryUp_HandlesEmptyGroup()
    {
        // Arrange
        var dbFactory = Services.GetService<IDbContextFactory<AppDbContext>>();
        using var db = await dbFactory.CreateDbContextAsync();

        var category = new DataPointCategory
        {
            Name = "Solo Category",
            Group = "Solo Group",
            Type = PointType.Bool,
            Index = 1
        };
        db.Categories.Add(category);
        await db.SaveChangesAsync();

        // Act - Try to move up when it's the only one
        await db.MoveCategoryUp(category);
        await db.SaveChangesAsync();

        // Assert - Index should remain unchanged
        category.Index.Should().Be(1);
    }

    [Fact]
    public async Task AddCategory_HandlesEmptyGroup()
    {
        // Arrange
        var dbFactory = Services.GetService<IDbContextFactory<AppDbContext>>();
        using var db = await dbFactory.CreateDbContextAsync();

        var category = new DataPointCategory
        {
            Name = "First Category",
            Group = "New Group",
            Type = PointType.Bool
        };

        // Act
        db.AddCategory(category);
        await db.SaveChangesAsync();

        // Assert - Should get index 1
        category.Index.Should().Be(1);
    }

    [Fact]
    public async Task GetOrCreateDayAndAddPoints_DoesNotDuplicatePoints()
    {
        // Test that calling GetOrCreateDayAndAddPoints multiple times
        // doesn't create duplicate points

        // Arrange
        var dbFactory = Services.GetService<IDbContextFactory<AppDbContext>>();
        var appDbSeeder = Services.GetService<AppDbSeeder>();
        appDbSeeder.SeedCategories();

        var date = new DateOnly(2024, 1, 1);

        // Act - Call twice
        using (var db = await dbFactory.CreateDbContextAsync())
        {
            await db.GetOrCreateDayAndAddPoints(date);
            await db.SaveChangesAsync();
        }

        using (var db = await dbFactory.CreateDbContextAsync())
        {
            await db.GetOrCreateDayAndAddPoints(date);
            await db.SaveChangesAsync();
        }

        // Assert - Each category should have exactly one point
        using (var db = await dbFactory.CreateDbContextAsync())
        {
            var day = db.Days.Include(d => d.Points).First(d => d.Date == date);
            var enabledCategories = db.Categories.Where(c => c.Enabled && !c.Deleted && c.Group != "Notes").ToList();

            foreach (var category in enabledCategories)
            {
                day.Points.Count(p => p.Category.Guid == category.Guid).Should().Be(1,
                    $"Category {category.Name} should have exactly one point");
            }
        }
    }

    [Fact]
    public async Task GetOrCreateDayAndAddPoints_WithDisabledCategories_DoesNotCreatePoints()
    {
        // Arrange
        var dbFactory = Services.GetService<IDbContextFactory<AppDbContext>>();
        var appDbSeeder = Services.GetService<AppDbSeeder>();
        appDbSeeder.SeedCategories();

        using var db = await dbFactory.CreateDbContextAsync();

        // Disable all categories
        foreach (var category in db.Categories)
        {
            category.Enabled = false;
        }

        await db.SaveChangesAsync();

        var date = new DateOnly(2024, 1, 1);

        // Act
        await db.GetOrCreateDayAndAddPoints(date);
        await db.SaveChangesAsync();

        // Assert - Day should be created but no points
        using (var db2 = await dbFactory.CreateDbContextAsync())
        {
            var day = db2.Days.Include(d => d.Points).First(d => d.Date == date);
            day.Should().NotBeNull();
            day.Points.Should().BeEmpty();
        }
    }
}
