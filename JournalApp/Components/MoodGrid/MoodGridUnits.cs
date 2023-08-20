using System.Globalization;

namespace JournalApp.MoodGrid;

public readonly record struct GridDay(int? Day, int Index, DateOnly? Date, string Emoji, string Color);

public readonly struct GridMonth
{
    private readonly DateTimeFormatInfo _dateTimeFormat = CultureInfo.CurrentCulture.DateTimeFormat;

    public GridMonth(int month, IEnumerable<DataPoint> points, IEnumerable<DateOnly> dates)
    {
        Month = month;
        Name = _dateTimeFormat.GetMonthName(Month);
        Dates = dates.ToList();
        GridDays = GetGridDays(points.ToHashSet()).ToList();
        DaysOfWeek = GetDaysOfWeek().ToList();
    }

    public int Month { get; }
    public string Name { get; }
    public IReadOnlyList<DateOnly> Dates { get; }
    public IReadOnlyList<GridDay> GridDays { get; }
    public IReadOnlyList<DayOfWeek> DaysOfWeek { get; }

    private static string GetMoodColor(string emoji) => emoji switch
    {
        // https://www.colorhexa.com/0091ea-to-ff6d00
        "😄" => "#0091ea",
        "😀" => "#2b8bc3",
        "🙂" => "#55859c",
        "😐" => "#807f75",
        "🙁" => "#aa794e",
        "😧" => "#d47327",
        "😢" => "#ff6d00",
        _ => "transparent"
    };

    private sealed record DataPointLookup(DataPoint Point, DateOnly Date);

    private IEnumerable<GridDay> GetGridDays(HashSet<DataPoint> points)
    {
        var firstDate = Dates[0];
        var daysInMonth = DateTime.DaysInMonth(firstDate.Year, firstDate.Month);

        // If the first day of the month is Thu, but the culture starts the week on Mon, the grid will have 3 preceding blank spaces, making the first index -2.
        var firstIndex = 1 - ((7 - ((_dateTimeFormat.FirstDayOfWeek - firstDate.DayOfWeek) % 7)) % 7);

        foreach (var gridIndex in Enumerable.Range(firstIndex, 7 * 6))
        {
            var day = gridIndex >= 1 && gridIndex <= daysInMonth ? gridIndex : (int?)null;

            // Bundle date during search to avoid additional DB lookups (thru DataPoint.Day).
            var lookup = points.Select(p => new DataPointLookup(p, p.Day.Date)).SingleOrDefault(l => l.Date.Day == gridIndex);

            // Make next search easier by removing uniquely dated point.
            if (lookup != null)
            {
                Debug.Assert(lookup.Date.Year == firstDate.Year && lookup.Date.Month == firstDate.Month, "Pool of data points is polluted from other months");
                points.Remove(lookup.Point);
            }

            var emoji = lookup?.Point?.Mood;
            var color = GetMoodColor(emoji);

            yield return new GridDay(day, gridIndex, lookup?.Date, emoji, color);
        }
    }

    private IEnumerable<DayOfWeek> GetDaysOfWeek()
    {
        foreach (var i in Enumerable.Range((int)_dateTimeFormat.FirstDayOfWeek, 7))
            yield return (DayOfWeek)(i % 7);
    }
}

public readonly struct GridYear
{
    public GridYear(int year, HashSet<DataPoint> allMoodPoints)
    {
        Year = year;
        GridMonths = GetGridMonths(allMoodPoints).ToList();
    }

    private IEnumerable<GridMonth> GetGridMonths(IEnumerable<DataPoint> allMoodPoints)
    {
        var startDate = new DateOnly(Year, 1, 1);
        var endDate = new DateOnly(Year, 12, 31);
        var dates = startDate.DatesTo(endDate).ToList();

        // Find all data points from the start of the year to at most tomorrow in system time.
        var tomorrow = DateOnly.FromDateTime(DateTime.Now).Next();
        var year = Year;
        var yearPoints = allMoodPoints.Where(x => x.Day.Date.Year == year && x.Day.Date <= tomorrow).ToHashSet();

        // Create months of the year.
        for (var i = 1; i <= 12; i++)
            yield return new GridMonth(i, yearPoints.Where(x => x.Day.Date.Month == i), dates.Where(x => x.Month == i));
    }

    public int Year { get; }
    public IReadOnlyList<GridMonth> GridMonths { get; }
}