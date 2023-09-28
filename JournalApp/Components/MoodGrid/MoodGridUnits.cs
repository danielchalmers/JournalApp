using System.Globalization;

namespace JournalApp.MoodGrid;

public readonly record struct GridDay(int Index, DateOnly? Date, string Emoji, string Color);

public readonly struct GridMonth
{
    private readonly CultureInfo _culture;

    public GridMonth(int month, CultureInfo culture, IDictionary<DateOnly, DataPoint> moodPoints, IEnumerable<DateOnly> dates)
    {
        Month = month;
        _culture = culture;
        Name = culture.DateTimeFormat.GetMonthName(Month);
        Dates = dates.ToList();
        GridDays = GetGridDays(moodPoints).ToList();
        DaysOfWeek = GetDaysOfWeek().ToList();
    }

    public int Month { get; }
    public string Name { get; }
    public IReadOnlyList<DateOnly> Dates { get; }
    public IReadOnlyList<GridDay> GridDays { get; }
    public IReadOnlyList<DayOfWeek> DaysOfWeek { get; }

    private static string GetMoodColor(string emoji) => emoji switch
    {
        // http://colorsafe.co/ - #373740.
        // https://colordesigner.io/gradient-generator - RGB Mode.
        "😄" => "#00baff",
        "😀" => "#2ba7d5",
        "🙂" => "#5593aa",
        "😐" => "#808080",
        "🙁" => "#aa6c55",
        "😧" => "#d5592b",
        "😢" => "#ff4500",
        _ => "transparent"
    };

    private IEnumerable<GridDay> GetGridDays(IDictionary<DateOnly, DataPoint> moodPoints)
    {
        var firstDate = Dates[0];
        var daysInMonth = DateTime.DaysInMonth(firstDate.Year, firstDate.Month);

        // If the first day of the month is Thu, but the culture starts the week on Mon, the grid will have 3 preceding blank spaces, making the first index -2.
        var firstIndex = 1 - ((7 - ((_culture.DateTimeFormat.FirstDayOfWeek - firstDate.DayOfWeek) % 7)) % 7);
        var indexes = Enumerable.Range(firstIndex, 7 * 6).ToList();

        foreach (var i in indexes)
        {
            // Bundle date during search to avoid additional DB lookups (thru DataPoint.Day).
            var date = i >= 1 && i <= daysInMonth ? new(firstDate.Year, firstDate.Month, i) : (DateOnly?)null;
            DataPoint point = null;
            _ = date.HasValue && moodPoints.TryGetValue(date.Value, out point);
            var emoji = point?.Mood;
            var color = GetMoodColor(emoji);

            yield return new GridDay(i, date, emoji, color);
        }
    }

    private IEnumerable<DayOfWeek> GetDaysOfWeek()
    {
        foreach (var i in Enumerable.Range((int)_culture.DateTimeFormat.FirstDayOfWeek, 7))
            yield return (DayOfWeek)(i % 7);
    }
}

public readonly struct GridYear
{
    private readonly CultureInfo _culture;

    public GridYear(int year, CultureInfo culture, IDictionary<DateOnly, DataPoint> moodPoints)
    {
        Year = year;
        _culture = culture;
        GridMonths = GetGridMonths(moodPoints).ToList();
    }

    private IEnumerable<GridMonth> GetGridMonths(IDictionary<DateOnly, DataPoint> moodPoints)
    {
        var startDate = new DateOnly(Year, 1, 1);
        var endDate = new DateOnly(Year, 12, 31);
        var dates = startDate.DatesTo(endDate).ToList();

        // Find all data points from the start of the year to at most tomorrow in system time.
        var tomorrow = DateOnly.FromDateTime(DateTime.Now).Next();
        var year = Year;

        // Create months of the year.
        for (var i = 1; i <= 12; i++)
            yield return new GridMonth(i, _culture, moodPoints.Where(x => x.Key.Month == i).ToDictionary(), dates.Where(x => x.Month == i));
    }

    public int Year { get; }
    public IReadOnlyList<GridMonth> GridMonths { get; }
}