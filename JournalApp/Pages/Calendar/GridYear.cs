using System.Globalization;

namespace JournalApp;

public readonly record struct GridYear
{
    private readonly CultureInfo _culture;

    public GridYear(int year, CultureInfo culture, Dictionary<DateOnly, DataPoint> moodPoints)
    {
        Year = year;
        _culture = culture;
        var streakInfo = StreakService.CalculateStreaks(moodPoints);
        GridMonths = GetGridMonths(moodPoints, streakInfo).ToList();
    }

    private IEnumerable<GridMonth> GetGridMonths(Dictionary<DateOnly, DataPoint> moodPoints, Dictionary<DateOnly, StreakInfo> streakInfo)
    {
        for (var i = 1; i <= 12; i++)
            yield return new GridMonth(Year, i, _culture, moodPoints.Where(x => x.Key.Month == i).ToDictionary(), streakInfo);
    }

    public int Year { get; }
    public IReadOnlyList<GridMonth> GridMonths { get; }
}
