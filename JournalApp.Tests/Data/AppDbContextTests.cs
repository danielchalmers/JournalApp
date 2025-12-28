using Microsoft.EntityFrameworkCore;

namespace JournalApp.Tests.Data;

public class AppDbContextTests : JaTestContext
{
    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        AddDbContext();
    }

    [Fact]
    public async Task GetOrCreateDayAndAddPoints_CreatesNewDay_WhenNotExists()
    {
        // Arrange
        var dbFactory = Services.GetService<IDbContextFactory<AppDbContext>>();
        var appDbSeeder = Services.GetService<AppDbSeeder>();
        appDbSeeder.SeedCategories();

        using var db = await dbFactory.CreateDbContextAsync();
        var date = new DateOnly(2024, 1, 1);

        // Act
        var day = await db.GetOrCreateDayAndAddPoints(date);
        await db.SaveChangesAsync();

        // Assert
        day.Should().NotBeNull();
        day.Date.Should().Be(date);
        db.Days.Should().Contain(d => d.Date == date);
    }

    [Fact]
    public async Task GetOrCreateDayAndAddPoints_ReturnsExistingDay_WhenExists()
    {
        // Arrange
        var dbFactory = Services.GetService<IDbContextFactory<AppDbContext>>();
        var appDbSeeder = Services.GetService<AppDbSeeder>();
        appDbSeeder.SeedCategories();

        using var db = await dbFactory.CreateDbContextAsync();
        var date = new DateOnly(2024, 1, 1);

        // Create day first
        var originalDay = await db.GetOrCreateDayAndAddPoints(date);
        await db.SaveChangesAsync();
        var originalGuid = originalDay.Guid;

        // Act - Get same day again
        var retrievedDay = await db.GetOrCreateDayAndAddPoints(date);
        await db.SaveChangesAsync();

        // Assert
        retrievedDay.Guid.Should().Be(originalGuid);
        db.Days.Count(d => d.Date == date).Should().Be(1);
    }

    [Fact]
    public async Task GetOrCreateDayAndAddPoints_AddsPointsForEnabledCategories()
    {
        // Arrange
        var dbFactory = Services.GetService<IDbContextFactory<AppDbContext>>();
        var appDbSeeder = Services.GetService<AppDbSeeder>();
        appDbSeeder.SeedCategories();

        using var db = await dbFactory.CreateDbContextAsync();
        var date = new DateOnly(2024, 1, 1);

        // Act
        var day = await db.GetOrCreateDayAndAddPoints(date);
        await db.SaveChangesAsync();

        // Assert
        var enabledCategories = db.Categories.Where(c => c.Enabled && !c.Deleted).ToList();
        enabledCategories.Should().NotBeEmpty();

        // Each enabled non-Notes category should have a point
        foreach (var category in enabledCategories.Where(c => c.Group != "Notes"))
        {
            day.Points.Should().Contain(p => p.Category.Guid == category.Guid);
        }
    }

    [Fact]
    public async Task GetOrCreateDayAndAddPoints_DoesNotAddPointsForDisabledCategories()
    {
        // Arrange
        var dbFactory = Services.GetService<IDbContextFactory<AppDbContext>>();
        var appDbSeeder = Services.GetService<AppDbSeeder>();
        appDbSeeder.SeedCategories();

        using var db = await dbFactory.CreateDbContextAsync();

        // Disable a category
        var categoryToDisable = db.Categories.First(c => c.Enabled && !c.Deleted);
        categoryToDisable.Enabled = false;
        await db.SaveChangesAsync();

        var date = new DateOnly(2024, 1, 1);

        // Act
        var day = await db.GetOrCreateDayAndAddPoints(date);
        await db.SaveChangesAsync();

        // Assert
        day.Points.Should().NotContain(p => p.Category.Guid == categoryToDisable.Guid);
    }

    [Fact]
    public async Task GetOrCreateDayAndAddPoints_DoesNotAddPointsForDeletedCategories()
    {
        // Arrange
        var dbFactory = Services.GetService<IDbContextFactory<AppDbContext>>();
        var appDbSeeder = Services.GetService<AppDbSeeder>();
        appDbSeeder.SeedCategories();

        using var db = await dbFactory.CreateDbContextAsync();

        // Delete a category
        var categoryToDelete = db.Categories.First(c => c.Enabled && !c.Deleted);
        categoryToDelete.Deleted = true;
        await db.SaveChangesAsync();

        var date = new DateOnly(2024, 1, 1);

        // Act
        var day = await db.GetOrCreateDayAndAddPoints(date);
        await db.SaveChangesAsync();

        // Assert
        day.Points.Should().NotContain(p => p.Category.Guid == categoryToDelete.Guid);
    }

    [Fact]
    public async Task GetMissingPoints_ReturnsEmptySet_WhenCategoryDisabled()
    {
        // Arrange
        var dbFactory = Services.GetService<IDbContextFactory<AppDbContext>>();
        var appDbSeeder = Services.GetService<AppDbSeeder>();
        appDbSeeder.SeedCategories();

        using var db = await dbFactory.CreateDbContextAsync();
        var day = Day.Create(new DateOnly(2024, 1, 1));
        var category = db.Categories.First();
        category.Enabled = false;

        // Act
        var missingPoints = db.GetMissingPoints(day, category, null);

        // Assert
        missingPoints.Should().BeEmpty();
    }

    [Fact]
    public async Task GetMissingPoints_ReturnsEmptySet_WhenCategoryDeleted()
    {
        // Arrange
        var dbFactory = Services.GetService<IDbContextFactory<AppDbContext>>();
        var appDbSeeder = Services.GetService<AppDbSeeder>();
        appDbSeeder.SeedCategories();

        using var db = await dbFactory.CreateDbContextAsync();
        var day = Day.Create(new DateOnly(2024, 1, 1));
        var category = db.Categories.First();
        category.Deleted = true;

        // Act
        var missingPoints = db.GetMissingPoints(day, category, null);

        // Assert
        missingPoints.Should().BeEmpty();
    }

    [Fact]
    public async Task GetMissingPoints_ReturnsPoint_WhenNoExistingPointForCategory()
    {
        // Arrange
        var dbFactory = Services.GetService<IDbContextFactory<AppDbContext>>();
        var appDbSeeder = Services.GetService<AppDbSeeder>();
        appDbSeeder.SeedCategories();

        using var db = await dbFactory.CreateDbContextAsync();
        var day = Day.Create(new DateOnly(2024, 1, 1));
        var category = db.Categories.First(c => c.Enabled && !c.Deleted && c.Group != "Notes");

        // Act
        var missingPoints = db.GetMissingPoints(day, category, null);

        // Assert
        missingPoints.Should().HaveCount(1);
        missingPoints.First().Category.Should().Be(category);
        missingPoints.First().Day.Should().Be(day);
    }

    [Fact]
    public async Task GetMissingPoints_ReturnsEmptySet_WhenPointAlreadyExists()
    {
        // Arrange
        var dbFactory = Services.GetService<IDbContextFactory<AppDbContext>>();
        var appDbSeeder = Services.GetService<AppDbSeeder>();
        appDbSeeder.SeedCategories();

        using var db = await dbFactory.CreateDbContextAsync();
        var day = Day.Create(new DateOnly(2024, 1, 1));
        var category = db.Categories.First(c => c.Enabled && !c.Deleted && c.Group != "Notes");

        // Add existing point
        var existingPoint = DataPoint.Create(day, category);
        day.Points.Add(existingPoint);

        // Act
        var missingPoints = db.GetMissingPoints(day, category, null);

        // Assert
        missingPoints.Should().BeEmpty();
    }

    [Fact]
    public async Task GetMissingPoints_SetsMedicationTaken_WhenEveryDaySinceIsBeforeDate()
    {
        // Arrange
        var dbFactory = Services.GetService<IDbContextFactory<AppDbContext>>();
        var appDbSeeder = Services.GetService<AppDbSeeder>();
        appDbSeeder.SeedCategories();

        using var db = await dbFactory.CreateDbContextAsync();
        var today = DateOnly.FromDateTime(DateTime.Now);
        var day = Day.Create(today);

        var medicationCategory = db.Categories.First(c => c.Type == PointType.Medication);
        medicationCategory.MedicationEveryDaySince = DateTime.Now.AddDays(-5);

        // Act
        var missingPoints = db.GetMissingPoints(day, medicationCategory, null);

        // Assert
        missingPoints.Should().HaveCount(1);
        missingPoints.First().Bool.Should().BeTrue();
    }

    [Fact]
    public async Task GetMissingPoints_DoesNotSetMedicationTaken_WhenEveryDaySinceIsAfterDate()
    {
        // Arrange
        var dbFactory = Services.GetService<IDbContextFactory<AppDbContext>>();
        var appDbSeeder = Services.GetService<AppDbSeeder>();
        appDbSeeder.SeedCategories();

        using var db = await dbFactory.CreateDbContextAsync();
        var yesterday = DateOnly.FromDateTime(DateTime.Now.AddDays(-1));
        var day = Day.Create(yesterday);

        var medicationCategory = db.Categories.First(c => c.Type == PointType.Medication);
        medicationCategory.MedicationEveryDaySince = DateTime.Now; // Today, so yesterday shouldn't be marked

        // Act
        var missingPoints = db.GetMissingPoints(day, medicationCategory, null);

        // Assert
        missingPoints.Should().HaveCount(1);
        missingPoints.First().Bool.Should().NotBe(true);
    }

    [Fact]
    public async Task AddCategory_AssignsIndexAutomatically_WhenNotSet()
    {
        // Arrange
        var dbFactory = Services.GetService<IDbContextFactory<AppDbContext>>();
        var appDbSeeder = Services.GetService<AppDbSeeder>();
        appDbSeeder.SeedCategories();

        using var db = await dbFactory.CreateDbContextAsync();
        var newCategory = new DataPointCategory
        {
            Name = "Test Category",
            Group = "Test Group",
            Type = PointType.Bool
        };

        // Act
        db.AddCategory(newCategory);
        await db.SaveChangesAsync();

        // Assert
        newCategory.Index.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task AddCategory_AssignsHighestIndexPlusOne_InSameGroup()
    {
        // Arrange
        var dbFactory = Services.GetService<IDbContextFactory<AppDbContext>>();
        var appDbSeeder = Services.GetService<AppDbSeeder>();
        appDbSeeder.SeedCategories();

        using var db = await dbFactory.CreateDbContextAsync();

        var testGroup = "Test Group";
        var category1 = new DataPointCategory
        {
            Name = "Category 1",
            Group = testGroup,
            Type = PointType.Bool
        };
        db.AddCategory(category1);
        await db.SaveChangesAsync();

        var category2 = new DataPointCategory
        {
            Name = "Category 2",
            Group = testGroup,
            Type = PointType.Bool
        };

        // Act
        db.AddCategory(category2);
        await db.SaveChangesAsync();

        // Assert
        category2.Index.Should().Be(category1.Index + 1);
    }

    [Fact]
    public async Task AddCategory_PreservesExplicitIndex_WhenSet()
    {
        // Arrange
        var dbFactory = Services.GetService<IDbContextFactory<AppDbContext>>();
        var appDbSeeder = Services.GetService<AppDbSeeder>();
        appDbSeeder.SeedCategories();

        using var db = await dbFactory.CreateDbContextAsync();
        var newCategory = new DataPointCategory
        {
            Name = "Test Category",
            Group = "Test Group",
            Type = PointType.Bool,
            Index = 42
        };

        // Act
        db.AddCategory(newCategory);
        await db.SaveChangesAsync();

        // Assert
        newCategory.Index.Should().Be(42);
    }

    [Fact]
    public async Task MoveCategoryUp_SwapsIndexes_WithCategoryAbove()
    {
        // Arrange
        var dbFactory = Services.GetService<IDbContextFactory<AppDbContext>>();
        // Don't seed categories - use a clean slate

        using var db = await dbFactory.CreateDbContextAsync();

        var testGroup = "Test Group Unique";
        var category1 = new DataPointCategory
        {
            Name = "Category 1",
            Group = testGroup,
            Type = PointType.Bool,
            Index = 1
        };
        var category2 = new DataPointCategory
        {
            Name = "Category 2",
            Group = testGroup,
            Type = PointType.Bool,
            Index = 2
        };

        db.Categories.Add(category1);
        db.Categories.Add(category2);
        await db.SaveChangesAsync();

        // Act - MoveCategoryUp decreases category2's index and increases category1's
        await db.MoveCategoryUp(category2);
        await db.SaveChangesAsync();

        // Assert - After swap, category2 should be first (1) and category1 should be second (2)
        category2.Index.Should().Be(1);
        category1.Index.Should().Be(2);
    }

    [Fact]
    public async Task MoveCategoryUp_DoesNothing_WhenCategoryIsFirst()
    {
        // Arrange
        var dbFactory = Services.GetService<IDbContextFactory<AppDbContext>>();
        var appDbSeeder = Services.GetService<AppDbSeeder>();
        appDbSeeder.SeedCategories();

        using var db = await dbFactory.CreateDbContextAsync();

        var testGroup = "Test Group";
        var category = new DataPointCategory
        {
            Name = "Category 1",
            Group = testGroup,
            Type = PointType.Bool
        };

        db.AddCategory(category);
        await db.SaveChangesAsync();

        var originalIndex = category.Index;

        // Act
        await db.MoveCategoryUp(category);
        await db.SaveChangesAsync();

        // Assert
        category.Index.Should().Be(originalIndex);
    }

    [Fact]
    public async Task FixCategoryIndexes_RemovesGaps_InIndexSequence()
    {
        // Arrange
        var dbFactory = Services.GetService<IDbContextFactory<AppDbContext>>();
        var appDbSeeder = Services.GetService<AppDbSeeder>();
        appDbSeeder.SeedCategories();

        using var db = await dbFactory.CreateDbContextAsync();

        var testGroup = "Test Group";
        var category1 = new DataPointCategory
        {
            Name = "Category 1",
            Group = testGroup,
            Type = PointType.Bool,
            Index = 1
        };
        var category2 = new DataPointCategory
        {
            Name = "Category 2",
            Group = testGroup,
            Type = PointType.Bool,
            Index = 5
        };
        var category3 = new DataPointCategory
        {
            Name = "Category 3",
            Group = testGroup,
            Type = PointType.Bool,
            Index = 10
        };

        db.Categories.Add(category1);
        db.Categories.Add(category2);
        db.Categories.Add(category3);
        await db.SaveChangesAsync();

        // Act
        db.FixCategoryIndexes();
        await db.SaveChangesAsync();

        // Assert
        category1.Index.Should().Be(1);
        category2.Index.Should().Be(2);
        category3.Index.Should().Be(3);
    }

    [Fact]
    public async Task FixCategoryIndexes_SetsDeletedCategoryIndexToZero()
    {
        // Arrange
        var dbFactory = Services.GetService<IDbContextFactory<AppDbContext>>();
        var appDbSeeder = Services.GetService<AppDbSeeder>();
        appDbSeeder.SeedCategories();

        using var db = await dbFactory.CreateDbContextAsync();

        var testGroup = "Test Group";
        var category1 = new DataPointCategory
        {
            Name = "Category 1",
            Group = testGroup,
            Type = PointType.Bool,
            Index = 1
        };
        var category2 = new DataPointCategory
        {
            Name = "Category 2",
            Group = testGroup,
            Type = PointType.Bool,
            Index = 2,
            Deleted = true
        };

        db.Categories.Add(category1);
        db.Categories.Add(category2);
        await db.SaveChangesAsync();

        // Act
        db.FixCategoryIndexes();
        await db.SaveChangesAsync();

        // Assert
        category1.Index.Should().Be(1);
        category2.Index.Should().Be(0);
    }

    [Fact]
    public async Task FixCategoryIndexes_HandlesMultipleGroups_Independently()
    {
        // Arrange
        var dbFactory = Services.GetService<IDbContextFactory<AppDbContext>>();
        var appDbSeeder = Services.GetService<AppDbSeeder>();
        appDbSeeder.SeedCategories();

        using var db = await dbFactory.CreateDbContextAsync();

        var group1Category1 = new DataPointCategory
        {
            Name = "G1 Cat 1",
            Group = "Group 1",
            Type = PointType.Bool,
            Index = 5
        };
        var group1Category2 = new DataPointCategory
        {
            Name = "G1 Cat 2",
            Group = "Group 1",
            Type = PointType.Bool,
            Index = 10
        };
        var group2Category1 = new DataPointCategory
        {
            Name = "G2 Cat 1",
            Group = "Group 2",
            Type = PointType.Bool,
            Index = 3
        };

        db.Categories.Add(group1Category1);
        db.Categories.Add(group1Category2);
        db.Categories.Add(group2Category1);
        await db.SaveChangesAsync();

        // Act
        db.FixCategoryIndexes();
        await db.SaveChangesAsync();

        // Assert
        group1Category1.Index.Should().Be(1);
        group1Category2.Index.Should().Be(2);
        group2Category1.Index.Should().Be(1);
    }

    [Fact]
    public async Task CreateNote_CreatesDataPointWithNotesCategory()
    {
        // Arrange
        var dbFactory = Services.GetService<IDbContextFactory<AppDbContext>>();
        var appDbSeeder = Services.GetService<AppDbSeeder>();
        appDbSeeder.SeedCategories();

        using var db = await dbFactory.CreateDbContextAsync();
        var day = Day.Create(new DateOnly(2024, 1, 1));

        // Act
        var note = db.CreateNote(day);

        // Assert
        note.Should().NotBeNull();
        note.Day.Should().Be(day);
        note.Category.Group.Should().Be("Notes");
        note.Type.Should().Be(PointType.Note);
    }

    [Fact]
    public async Task CreateNote_ThrowsException_WhenNotesCategoryNotFound()
    {
        // Arrange
        var dbFactory = Services.GetService<IDbContextFactory<AppDbContext>>();
        // Don't seed categories

        using var db = await dbFactory.CreateDbContextAsync();
        var day = Day.Create(new DateOnly(2024, 1, 1));

        // Act & Assert
        var act = () => db.CreateNote(day);
        act.Should().Throw<InvalidOperationException>();
    }
}
