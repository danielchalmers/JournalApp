using System.Globalization;

namespace JournalApp;

public readonly record struct GridMonth
{
    private readonly CultureInfo _culture;

    public GridMonth(int year, int month, CultureInfo culture, Dictionary<DateOnly, DataPoint> moodPoints)
    {
        Year = year;
        Month = month;
        _culture = culture;
        Name = culture.DateTimeFormat.GetMonthName(Month);
        GridDays = GetGridDays(moodPoints).ToList();
        DaysOfWeek = GetDaysOfWeek().ToList();
    }

    public int Year { get; }
    public int Month { get; }
    public string Name { get; }
    public IReadOnlyList<GridDay> GridDays { get; }
    public IReadOnlyList<DayOfWeek> DaysOfWeek { get; }

    private IEnumerable<GridDay> GetGridDays(Dictionary<DateOnly, DataPoint> moodPoints)
    {
        var firstDate = new DateOnly(Year, Month, 1);
        var daysInMonth = DateTime.DaysInMonth(Year, Month);

        // The offset for the index of the first day.
        // If the first day of the month is Thu, but the culture starts the week on Mon, the grid will have 3 preceding blank spaces, making the offset -2.
        var offset = 1 - ((7 - ((_culture.DateTimeFormat.FirstDayOfWeek - firstDate.DayOfWeek) % 7)) % 7);

        // Create up to 6 rows of 7 squares each, with real days shown where index lines up.
        foreach (var i in Enumerable.Range(offset, 7 * 6))
        {
            DateOnly? date = null;
            DataPoint point = null;

            // If the index is a valid day number, assign the date and corresponding point.
            if (i >= 1 && i <= daysInMonth)
            {
                date = new(firstDate.Year, firstDate.Month, i);
                point = moodPoints.GetValueOrDefault(date.Value);
            }

            // Don't start a new row if we've already gone past the last valid day.
            if (i == offset + 35 && date == null)
                yield break;

            yield return new GridDay(i, date, point?.Mood);
        }
    }

    private IEnumerable<DayOfWeek> GetDaysOfWeek() =>
        Enumerable.Range((int)_culture.DateTimeFormat.FirstDayOfWeek, 7).Select(i => (DayOfWeek)(i % 7));
}
