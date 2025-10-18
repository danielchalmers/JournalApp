namespace JournalApp;

public class CalendarService(ILogger<CalendarService> logger, IDbContextFactory<AppDbContext> DbFactory)
{
    private DataPointCategory _moodCategory;

    public async Task<GridYear> CreateGridYear(int year)
    {
        logger.LogDebug("Loading grid year {Year}", year);
        var sw = Stopwatch.StartNew();
        var tomorrow = DateOnly.FromDateTime(DateTime.Now).Next();
        await using var db = await DbFactory.CreateDbContextAsync();

        _moodCategory ??= await db.Categories.SingleOrDefaultAsync(x => x.Guid == new Guid("D90D89FB-F5B9-47CF-AE4E-3EC0D635E783"));

        var moodPoints = new Dictionary<DateOnly, DataPoint>();
        if (_moodCategory == null || !_moodCategory.Enabled)
        {
            logger.LogError("Mood category missing or disabled; skipping mood point load");
        }
        else
        {
            var queryStopwatch = Stopwatch.StartNew();
            var query = db.Points
                .Where(p => !p.Deleted && p.Day.Date.Year == year && p.Day.Date <= tomorrow && p.Category.Guid == _moodCategory.Guid)
                .Select(
                    p => new
                    {
                        Date = p.Day.Date,
                        Point = p,
                    }
                );

            // Sort into dictionary here instead of during the query so we can handle
            // duplicate dates, if that has erroneously happened, without crashing.
            foreach (var x in query)
                moodPoints[x.Date] = x.Point;

            queryStopwatch.Stop();
            logger.LogDebug("Loaded {PointCount} mood points in {ElapsedMilliseconds}ms", moodPoints.Count, queryStopwatch.ElapsedMilliseconds);
        }

        var gridYear = new GridYear(year, System.Globalization.CultureInfo.CurrentCulture, moodPoints);
        sw.Stop();
        logger.LogInformation("Created grid year {Year} in {ElapsedMilliseconds}ms (mood points: {PointCount})", year, sw.ElapsedMilliseconds, moodPoints.Count);

        return gridYear;
    }
}
