using System.Globalization;

namespace JournalApp.MoodGrid;

public readonly record struct GridDay(int? Day, int Index, DateOnly? Date, string Emoji, string Color);

public readonly struct GridMonth
{
    private readonly DateTimeFormatInfo _dateTimeFormat = CultureInfo.CurrentCulture.DateTimeFormat;

    public GridMonth(int month, HashSet<DataPoint> dataPoints, IEnumerable<DateOnly> dates)
    {
        Month = month;
        Name = _dateTimeFormat.GetMonthName(Month);
        DataPoints = dataPoints;
        Dates = dates.ToList();
        GridDays = GetGridDays().ToList();
        DaysOfWeek = GetDaysOfWeek().ToList();
    }

    public int Month { get; }
    public string Name { get; }
    public HashSet<DataPoint> DataPoints { get; }
    public IReadOnlyList<DateOnly> Dates { get; }
    public IReadOnlyList<GridDay> GridDays { get; }
    public IReadOnlyList<DayOfWeek> DaysOfWeek { get; }

    private string GetMoodColor(string emoji) => emoji switch
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

    private record DataPointLookup(DataPoint Point, DateOnly Date);

    private IEnumerable<GridDay> GetGridDays()
    {
        var tomorrow = DateOnly.FromDateTime(DateTime.Now).AddDays(1);
        var firstDate = Dates.First();
        // If the first day of the month is Thu, but the culture starts the week on Mon, the grid will have 3 preceding blank spaces, making the first index -2.
        var firstIndex = 1 - ((7 - ((_dateTimeFormat.FirstDayOfWeek - firstDate.DayOfWeek) % 7)) % 7);
        var daysInMonth = DateTime.DaysInMonth(firstDate.Year, firstDate.Month);

        foreach (var gridIndex in Enumerable.Range(firstIndex, 7 * 6))
        {
            var day = gridIndex >= 1 && gridIndex <= daysInMonth ? gridIndex : (int?)null;

            // Bundle date during search to avoid additional DB lookups (thru DataPoint.Day).
            var lookup = DataPoints.Select(p =>
            {
                var date = p.Day.Date;
                return new DataPointLookup(p, date);
            }).SingleOrDefault(l => l.Date.Month == firstDate.Month && l.Date.Day == gridIndex && l.Date <= tomorrow);

            // Make next search easier by removing uniquely dated point.
            if (lookup != null)
                DataPoints.Remove(lookup.Point);

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
    public GridYear(int year, HashSet<DataPoint> dataPoints)
    {
        Year = year;
        DataPoints = dataPoints;
        GridMonths = GetGridMonths().ToList();
    }

    private IEnumerable<GridMonth> GetGridMonths()
    {
        var startDate = new DateOnly(Year, 1, 1);
        var endDate = new DateOnly(Year, 12, 31);
        var dates = startDate.DatesTo(endDate).ToList();

        for (var i = 1; i <= 12; i++)
            yield return new GridMonth(i, DataPoints, dates.Where(x => x.Month == i));
    }

    public int Year { get; }
    public HashSet<DataPoint> DataPoints { get; }
    public IReadOnlyList<GridMonth> GridMonths { get; }
}