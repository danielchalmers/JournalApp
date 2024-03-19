using MudBlazor.Utilities;

namespace JournalApp;

public class CalendarService(ILogger<CalendarService> logger, IDbContextFactory<AppDbContext> DbFactory, IPreferences preferences)
{
    private DataPointCategory _moodCategory;
    private Dictionary<string, string> _moodColors;

    private string MoodPalettePreference
    {
        get
        {
            var palette = preferences.Get("mood_palette", string.Empty);

            if (string.IsNullOrEmpty(palette))
                palette = "#6bdbe7"; // Tetradic to our primary purple.

            return palette;
        }
    }

    public MudColor PrimaryColor
    {
        get => MoodPalettePreference;
        set
        {
            preferences.Set("mood_palette", value.Value[..^2]);
            GenerateColors();
        }
    }

    public string GetMoodColor(string emoji)
    {
        if (_moodColors == null)
            GenerateColors();

        if (string.IsNullOrEmpty(emoji) || !_moodColors.TryGetValue(emoji, out var color))
            return "transparent";
        else
            return color;
    }

    public async Task<GridYear> CreateGridYear(int year)
    {
        logger.LogDebug($"Loading year {year}");
        var sw = Stopwatch.StartNew();
        var tomorrow = DateOnly.FromDateTime(DateTime.Now).Next();
        await using var db = await DbFactory.CreateDbContextAsync();

        _moodCategory ??= await db.Categories.SingleOrDefaultAsync(x => x.Guid == new Guid("D90D89FB-F5B9-47CF-AE4E-3EC0D635E783"));

        var moodPoints = new Dictionary<DateOnly, DataPoint>();
        if (_moodCategory == null || !_moodCategory.Enabled)
        {
            logger.LogError("Mood category doesn't exist or is disabled so we won't load any points.");
        }
        else
        {
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

            logger.LogDebug($"Found {moodPoints.Count} mood points");
        }

        var gridYear = new GridYear(year, System.Globalization.CultureInfo.CurrentCulture, moodPoints);
        logger.LogDebug($"Created grid year {year} in {sw.ElapsedMilliseconds}ms");

        return gridYear;
    }

    private void GenerateColors()
    {
        var emojis = DataPoint.Moods.Where(x => x != "🤔").ToList();
        var primary = PrimaryColor.ToMauiColor();
        var complementary = primary.GetComplementary();

        _moodColors = [];
        for (var i = 0; i < emojis.Count; i++)
        {
            var p = i / (emojis.Count - 1f);
            var c = ColorUtil.GetGradientColor(primary, complementary, p);

            _moodColors.Add(emojis[i], c.ToHex());
        }

        logger.LogInformation($"Primary color: {primary.ToHex()}");
        logger.LogInformation($"Palette: {string.Join(",", _moodColors)}");
    }
}
