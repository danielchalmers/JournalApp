namespace JournalApp;

public static class DateUtil
{
    public static IEnumerable<DateOnly> DatesTo(this DateOnly startDate, DateOnly endDate)
    {
        for (var date = startDate; date <= endDate; date = date.AddDays(1))
            yield return date;
    }

    public static DateOnly Next(this DateOnly dateOnly) => DateOnly.FromDayNumber(dateOnly.DayNumber + 1);

    public static DateOnly Previous(this DateOnly dateOnly) => DateOnly.FromDayNumber(dateOnly.DayNumber - 1);
}
