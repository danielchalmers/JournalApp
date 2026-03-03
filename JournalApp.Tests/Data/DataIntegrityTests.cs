using Microsoft.EntityFrameworkCore;

namespace JournalApp.Tests.Data;

public class DataIntegrityTests : JaTestContext
{
    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        AddDbContext();
    }

    [Fact(Skip = "ISSUE-TEST-004: FK enforcement differs on sqlite in-memory; provider=sqlite-inmemory; revisit=2026-06-01")]
    public Task DataPoint_RequiresCategory() => Task.CompletedTask;

    [Fact(Skip = "ISSUE-TEST-005: FK enforcement differs on sqlite in-memory; provider=sqlite-inmemory; revisit=2026-06-01")]
    public Task DataPoint_RequiresDay() => Task.CompletedTask;

    [Fact(Skip = "ISSUE-TEST-006: cascade semantics differ on sqlite in-memory; provider=sqlite-inmemory; revisit=2026-06-01")]
    public Task DeleteDay_DoesNotCascadeDeletePoints() => Task.CompletedTask;

    [Fact(Skip = "ISSUE-TEST-007: cascade semantics differ on sqlite in-memory; provider=sqlite-inmemory; revisit=2026-06-01")]
    public Task DeleteCategory_DoesNotCascadeDeletePoints() => Task.CompletedTask;

    [Fact]
    public async Task Category_DuplicateGuidsNotAllowed()
    {
        var dbFactory = Services.GetService<IDbContextFactory<AppDbContext>>();
        await using var db = await dbFactory.CreateDbContextAsync();

        var guid = Guid.NewGuid();
        db.Categories.Add(new DataPointCategory
        {
            Guid = guid,
            Name = "Category 1",
            Group = "Test",
            Type = PointType.Bool
        });
        await db.SaveChangesAsync();

        await using var db2 = await dbFactory.CreateDbContextAsync();
        db2.Categories.Add(new DataPointCategory
        {
            Guid = guid,
            Name = "Category 2",
            Group = "Test",
            Type = PointType.Bool
        });

        var act = async () => await db2.SaveChangesAsync();
        await act.Should().ThrowAsync<DbUpdateException>();
    }

    [Fact]
    public async Task GetMissingPoints_HandlesNullRandom()
    {
        var dbFactory = Services.GetService<IDbContextFactory<AppDbContext>>();
        var appDbSeeder = Services.GetService<AppDbSeeder>();
        appDbSeeder.SeedCategories();

        await using var db = await dbFactory.CreateDbContextAsync();
        var day = Day.Create(new DateOnly(2024, 1, 1));
        var category = db.Categories.First(c => c.Enabled && !c.Deleted && c.Group != "Notes");

        var points = db.GetMissingPoints(day, category, null);

        points.Should().HaveCount(1);
        var point = points.First();
        point.Mood.Should().BeNull();
        point.Number.Should().BeNull();
        point.Bool.Should().BeNull();
    }

    [Fact]
    public async Task GetMissingPoints_WithRandomMoodCategory_GeneratesMood()
    {
        var dbFactory = Services.GetService<IDbContextFactory<AppDbContext>>();
        var appDbSeeder = Services.GetService<AppDbSeeder>();
        appDbSeeder.SeedCategories();

        await using var db = await dbFactory.CreateDbContextAsync();
        var day = Day.Create(new DateOnly(2024, 1, 1));
        var category = db.Categories.First(c => c.Type == PointType.Mood && c.Enabled && !c.Deleted);

        var points = db.GetMissingPoints(day, category, new Random(42));

        points.Should().HaveCount(1);
        points.Single().Mood.Should().NotBeNull();
    }

    [Fact]
    public async Task GetMissingPoints_WithRandomNumberCategory_GeneratesNumber()
    {
        var dbFactory = Services.GetService<IDbContextFactory<AppDbContext>>();
        var appDbSeeder = Services.GetService<AppDbSeeder>();
        appDbSeeder.SeedCategories();

        await using var db = await dbFactory.CreateDbContextAsync();
        var day = Day.Create(new DateOnly(2024, 1, 1));
        var category = db.Categories.First(c => c.Type == PointType.Number && c.Enabled && !c.Deleted);

        var points = db.GetMissingPoints(day, category, new Random(42));

        points.Should().HaveCount(1);
        points.Single().Number.Should().NotBeNull();
    }

    [Fact]
    public async Task DataPoint_PreservesGuidOnSave()
    {
        var dbFactory = Services.GetService<IDbContextFactory<AppDbContext>>();
        var appDbSeeder = Services.GetService<AppDbSeeder>();
        appDbSeeder.SeedCategories();

        await using var db = await dbFactory.CreateDbContextAsync();
        var day = Day.Create(new DateOnly(2024, 1, 1));
        db.Days.Add(day);

        var category = db.Categories.First();
        var point = DataPoint.Create(day, category);
        var originalGuid = point.Guid;
        db.Points.Add(point);

        await db.SaveChangesAsync();

        point.Guid.Should().NotBe(Guid.Empty);
        if (originalGuid != Guid.Empty)
        {
            point.Guid.Should().Be(originalGuid);
        }
    }

    [Fact]
    public async Task MedicationDose_CanBeNull()
    {
        var dbFactory = Services.GetService<IDbContextFactory<AppDbContext>>();
        var appDbSeeder = Services.GetService<AppDbSeeder>();
        appDbSeeder.SeedCategories();

        await using var db = await dbFactory.CreateDbContextAsync();
        var day = Day.Create(new DateOnly(2024, 1, 1));
        db.Days.Add(day);

        var category = db.Categories.First(c => c.Type == PointType.Medication);
        var point = DataPoint.Create(day, category);
        point.MedicationDose = null;
        db.Points.Add(point);

        await db.SaveChangesAsync();
        point.MedicationDose.Should().BeNull();
    }

    [Fact]
    public async Task Category_CanHaveNullMedicationEveryDaySince()
    {
        var dbFactory = Services.GetService<IDbContextFactory<AppDbContext>>();
        await using var db = await dbFactory.CreateDbContextAsync();

        var category = new DataPointCategory
        {
            Name = "Test Med",
            Group = "Medications",
            Type = PointType.Medication,
            MedicationEveryDaySince = null
        };

        db.AddCategory(category);

        await db.SaveChangesAsync();
        category.MedicationEveryDaySince.Should().BeNull();
    }

    [Fact]
    public async Task DataPoint_HandlesNullText()
    {
        var dbFactory = Services.GetService<IDbContextFactory<AppDbContext>>();
        var appDbSeeder = Services.GetService<AppDbSeeder>();
        appDbSeeder.SeedCategories();

        await using var db = await dbFactory.CreateDbContextAsync();
        var day = Day.Create(new DateOnly(2024, 1, 1));
        db.Days.Add(day);

        var category = db.Categories.First();
        var point = DataPoint.Create(day, category);
        point.Text = null;
        db.Points.Add(point);

        await db.SaveChangesAsync();
        point.Text.Should().BeNull();
    }

    [Fact]
    public async Task Day_HandlesFutureDate()
    {
        var dbFactory = Services.GetService<IDbContextFactory<AppDbContext>>();
        await using var db = await dbFactory.CreateDbContextAsync();

        var futureDate = new DateOnly(2034, 1, 1);
        var day = Day.Create(futureDate);
        db.Days.Add(day);

        await db.SaveChangesAsync();
        day.Date.Should().Be(futureDate);
    }

    [Fact]
    public async Task Day_HandlesVeryOldDate()
    {
        var dbFactory = Services.GetService<IDbContextFactory<AppDbContext>>();
        await using var db = await dbFactory.CreateDbContextAsync();

        var oldDate = new DateOnly(1900, 1, 1);
        var day = Day.Create(oldDate);
        db.Days.Add(day);

        await db.SaveChangesAsync();
        day.Date.Should().Be(oldDate);
    }

    [Fact]
    public async Task DataPoint_HandlesNullMood()
    {
        var dbFactory = Services.GetService<IDbContextFactory<AppDbContext>>();
        var appDbSeeder = Services.GetService<AppDbSeeder>();
        appDbSeeder.SeedCategories();

        await using var db = await dbFactory.CreateDbContextAsync();
        var day = Day.Create(new DateOnly(2024, 1, 1));
        db.Days.Add(day);

        var category = db.Categories.First(c => c.Type == PointType.Mood);
        var point = DataPoint.Create(day, category);
        point.Mood = null;
        db.Points.Add(point);

        await db.SaveChangesAsync();
        point.Mood.Should().BeNull();
    }

    [Fact]
    public async Task DataPoint_HandlesBoundaryNumberValues()
    {
        var dbFactory = Services.GetService<IDbContextFactory<AppDbContext>>();
        var appDbSeeder = Services.GetService<AppDbSeeder>();
        appDbSeeder.SeedCategories();

        await using var db = await dbFactory.CreateDbContextAsync();
        var day = Day.Create(new DateOnly(2024, 1, 1));
        db.Days.Add(day);

        var category = db.Categories.First(c => c.Type == PointType.Number);

        var point1 = DataPoint.Create(day, category);
        point1.Number = double.MaxValue;
        db.Points.Add(point1);

        var point2 = DataPoint.Create(day, category);
        point2.Number = double.MinValue;
        db.Points.Add(point2);

        var point3 = DataPoint.Create(day, category);
        point3.Number = 0;
        db.Points.Add(point3);

        var point4 = DataPoint.Create(day, category);
        point4.Number = -999999.999;
        db.Points.Add(point4);

        await db.SaveChangesAsync();

        point1.Number.Should().Be(double.MaxValue);
        point2.Number.Should().Be(double.MinValue);
        point3.Number.Should().Be(0);
        point4.Number.Should().Be(-999999.999);
    }

    [Fact]
    public async Task DataPoint_HandlesBoundaryDecimalValues()
    {
        var dbFactory = Services.GetService<IDbContextFactory<AppDbContext>>();
        var appDbSeeder = Services.GetService<AppDbSeeder>();
        appDbSeeder.SeedCategories();

        await using var db = await dbFactory.CreateDbContextAsync();
        var day = Day.Create(new DateOnly(2024, 1, 1));
        db.Days.Add(day);

        var category = db.Categories.First(c => c.Type == PointType.Sleep);

        var point1 = DataPoint.Create(day, category);
        point1.SleepHours = 24m;
        db.Points.Add(point1);

        var point2 = DataPoint.Create(day, category);
        point2.SleepHours = 0m;
        db.Points.Add(point2);

        var point3 = DataPoint.Create(day, category);
        point3.SleepHours = 12.5m;
        db.Points.Add(point3);

        await db.SaveChangesAsync();

        point1.SleepHours.Should().Be(24m);
        point2.SleepHours.Should().Be(0m);
        point3.SleepHours.Should().Be(12.5m);
    }

    [Fact]
    public async Task Category_HandlesNullMedicationUnit()
    {
        var dbFactory = Services.GetService<IDbContextFactory<AppDbContext>>();
        await using var db = await dbFactory.CreateDbContextAsync();

        var category = new DataPointCategory
        {
            Name = "Test Med",
            Group = "Medications",
            Type = PointType.Medication,
            MedicationDose = 100m,
            MedicationUnit = null
        };

        db.AddCategory(category);

        await db.SaveChangesAsync();
        category.MedicationUnit.Should().BeNull();
    }

    [Fact]
    public async Task DataPoint_PreservesCreatedAtTimestamp()
    {
        var dbFactory = Services.GetService<IDbContextFactory<AppDbContext>>();
        var appDbSeeder = Services.GetService<AppDbSeeder>();
        appDbSeeder.SeedCategories();

        await using var db = await dbFactory.CreateDbContextAsync();
        var day = Day.Create(new DateOnly(2024, 1, 1));
        db.Days.Add(day);

        var category = db.Categories.First();
        var point = DataPoint.Create(day, category);
        var originalTimestamp = point.CreatedAt;

        db.Points.Add(point);
        await db.SaveChangesAsync();

        var pointGuid = point.Guid;
        db.Entry(point).State = EntityState.Detached;

        var reloadedPoint = await db.Points.FirstAsync(p => p.Guid == pointGuid);
        reloadedPoint.CreatedAt.Should().BeCloseTo(originalTimestamp, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task GetOrCreateDayAndAddPoints_WithDisabledCategories_DoesNotCreatePoints()
    {
        var dbFactory = Services.GetService<IDbContextFactory<AppDbContext>>();
        var appDbSeeder = Services.GetService<AppDbSeeder>();
        appDbSeeder.SeedCategories();

        await using var db = await dbFactory.CreateDbContextAsync();

        foreach (var category in db.Categories)
        {
            category.Enabled = false;
        }

        await db.SaveChangesAsync();

        var date = new DateOnly(2024, 1, 1);
        await db.GetOrCreateDayAndAddPoints(date);
        await db.SaveChangesAsync();

        await using var db2 = await dbFactory.CreateDbContextAsync();
        var day = db2.Days.Include(d => d.Points).First(d => d.Date == date);
        day.Points.Should().BeEmpty();
    }

    [Fact]
    public async Task DataPoint_WithDeletedFlag_CanBeQueried()
    {
        var dbFactory = Services.GetService<IDbContextFactory<AppDbContext>>();
        var appDbSeeder = Services.GetService<AppDbSeeder>();
        appDbSeeder.SeedCategories();

        await using var db = await dbFactory.CreateDbContextAsync();
        var day = Day.Create(new DateOnly(2024, 1, 1));
        db.Days.Add(day);

        var category = db.Categories.First();
        var point = DataPoint.Create(day, category);
        point.Deleted = true;

        db.Points.Add(point);
        await db.SaveChangesAsync();

        var deletedPoints = db.Points.Where(p => p.Deleted).ToList();

        deletedPoints.Should().HaveCount(1);
        deletedPoints.First().Guid.Should().Be(point.Guid);
    }
}
