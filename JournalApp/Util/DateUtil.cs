namespace JournalApp;

public static class DateUtil
{
    public static DateOnly GetNextDate(this DateOnly dateOnly) => DateOnly.FromDayNumber(dateOnly.DayNumber + 1);

    public static DateOnly GetPreviousDate(this DateOnly dateOnly) => DateOnly.FromDayNumber(dateOnly.DayNumber - 1);
}
