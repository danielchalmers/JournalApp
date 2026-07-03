using Microsoft.EntityFrameworkCore;

namespace JournalApp.Tests.Data;

/// <summary>
/// CalendarService.CreateGridYear loads the year's mood points into a grid.
/// Its compound filter (not deleted, right year, not in the future, mood category only) and its deliberate tolerance of duplicate dates are defensive logic that nothing else exercises.
/// </summary>
public class CalendarServiceTests : JaTestContext
{
    private static readonly Guid MoodCategoryGuid = new("D90D89FB-F5B9-47CF-AE4E-3EC0D635E783");

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        AddDbContext();
    }

    private async Task AddMoodPoint(DateOnly date, string mood, bool deleted = false)
    {
        var dbFactory = Services.GetService<IDbContextFactory<AppDbContext>>();
        await using var db = await dbFactory.CreateDbContextAsync();
        var day = Day.Create(date);
        db.Days.Add(day);
        var moodCategory = db.Categories.Single(c => c.Guid == MoodCategoryGuid);
        var point = DataPoint.Create(day, moodCategory);
        point.Mood = mood;
        point.Deleted = deleted;
        db.Points.Add(point);
        await db.SaveChangesAsync();
    }

    private async Task AddNonMoodPoint(DateOnly date)
    {
        var dbFactory = Services.GetService<IDbContextFactory<AppDbContext>>();
        await using var db = await dbFactory.CreateDbContextAsync();
        var day = Day.Create(date);
        db.Days.Add(day);
        var category = db.Categories.First(c => c.Type == PointType.Sleep);
        db.Points.Add(DataPoint.Create(day, category));
        await db.SaveChangesAsync();
    }

    [Fact]
    public async Task CreateGridYear_PlacesMoodEmojiOnTheCorrectDay()
    {
        Services.GetService<AppDbSeeder>().SeedCategories();
        await AddMoodPoint(new DateOnly(2024, 6, 10), "😀");

        var gridYear = await Services.GetService<CalendarService>().CreateGridYear(2024);

        gridYear.GridMonths[5].GridDays.Single(d => d.Date == new DateOnly(2024, 6, 10)).Emoji.Should().Be("😀");
    }

    [Fact]
    public async Task CreateGridYear_ExcludesDeletedWrongYearAndNonMoodPoints()
    {
        Services.GetService<AppDbSeeder>().SeedCategories();
        await AddMoodPoint(new DateOnly(2024, 6, 10), "😀");              // included
        await AddMoodPoint(new DateOnly(2024, 6, 11), "😢", deleted: true); // excluded: deleted
        await AddMoodPoint(new DateOnly(2023, 6, 10), "🙂");              // excluded: wrong year
        await AddNonMoodPoint(new DateOnly(2024, 6, 12));                 // excluded: not the mood category

        var gridYear = await Services.GetService<CalendarService>().CreateGridYear(2024);

        var withEmoji = gridYear.GridMonths.SelectMany(m => m.GridDays).Where(d => d.Emoji != null).ToList();
        withEmoji.Should().ContainSingle();
        gridYear.GridMonths[5].GridDays.Single(d => d.Date == new DateOnly(2024, 6, 10)).Emoji.Should().Be("😀");
    }

    [Fact]
    public async Task CreateGridYear_DoesNotThrow_OnDuplicateDates()
    {
        Services.GetService<AppDbSeeder>().SeedCategories();
        // Two mood points on the same date is erroneous but must not crash the grid.
        await AddMoodPoint(new DateOnly(2024, 6, 10), "😀");
        await AddMoodPoint(new DateOnly(2024, 6, 10), "😢");

        var calendarService = Services.GetService<CalendarService>();
        var act = () => calendarService.CreateGridYear(2024);

        var gridYear = (await act.Should().NotThrowAsync()).Subject;
        gridYear.GridMonths[5].GridDays.Single(d => d.Date == new DateOnly(2024, 6, 10)).Emoji.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateGridYear_ReturnsEmptyGrid_WhenMoodCategoryDisabled()
    {
        var dbFactory = Services.GetService<IDbContextFactory<AppDbContext>>();
        Services.GetService<AppDbSeeder>().SeedCategories();
        await AddMoodPoint(new DateOnly(2024, 6, 10), "😀");

        // Disable the mood category before the service caches it on its first (and only) call.
        await using (var db = await dbFactory.CreateDbContextAsync())
        {
            db.Categories.Single(c => c.Guid == MoodCategoryGuid).Enabled = false;
            await db.SaveChangesAsync();
        }

        var gridYear = await Services.GetService<CalendarService>().CreateGridYear(2024);

        gridYear.GridMonths.SelectMany(m => m.GridDays).Should().OnlyContain(d => d.Emoji == null);
    }
}
