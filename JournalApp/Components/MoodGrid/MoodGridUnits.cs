using System.Globalization;

namespace JournalApp.MoodGrid;

public readonly record struct GridDay(int Index, DateOnly? Date, string Emoji);

public readonly struct GridMonth
{
    private readonly CultureInfo _culture;

    public GridMonth(int month, CultureInfo culture, Dictionary<DateOnly, DataPoint> moodPoints, IEnumerable<DateOnly> dates)
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

    private IEnumerable<GridDay> GetGridDays(Dictionary<DateOnly, DataPoint> moodPoints)
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
            var point = date.HasValue ? moodPoints.GetValueOrDefault(date.Value) : null;
            var emoji = point?.Mood;

            yield return new GridDay(i, date, emoji);
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

    public GridYear(int year, CultureInfo culture, Dictionary<DateOnly, DataPoint> moodPoints)
    {
        Year = year;
        _culture = culture;
        GridMonths = GetGridMonths(moodPoints).ToList();
    }

    private IEnumerable<GridMonth> GetGridMonths(Dictionary<DateOnly, DataPoint> moodPoints)
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