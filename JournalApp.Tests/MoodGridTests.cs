using System.Globalization;
using JournalApp.MoodGrid;

namespace JournalApp.Tests;

public class MoodGridTests
{
    [Fact]
    public void CalendarDates()
    {
        var year = 2023;
        var gridYear = new GridYear(year, CultureInfo.InvariantCulture, new());

        Assert.Equal(12, gridYear.GridMonths.Count);

        var dayCounts = new List<int>()
        {
            31, // January.
            28, // February.
            31, // March.
            30, // April.
            31, // May.
            30, // June.
            31, // July.
            31, // August.
            30, // September.
            31, // October.
            30, // November
            31, // December.
        };

        for (var i = 0; i < gridYear.GridMonths.Count; i++)
        {
            var gridMonth = gridYear.GridMonths[i];

            var expectedDates = Enumerable.Range(1, dayCounts[i]).Select(d => new DateOnly(year, gridMonth.Month, d)).ToList();
            Assert.Equal(expectedDates, gridMonth.Dates);
        }
    }

    [Fact]
    public void DaysOfWeekInvariantCulture()
    {
        var gridYear = new GridYear(2023, CultureInfo.InvariantCulture, new());

        for (var i = 0; i < gridYear.GridMonths.Count; i++)
        {
            var gridMonth = gridYear.GridMonths[i];

            Assert.Equal(new List<DayOfWeek> {
                DayOfWeek.Sunday,
                DayOfWeek.Monday,
                DayOfWeek.Tuesday,
                DayOfWeek.Wednesday,
                DayOfWeek.Thursday,
                DayOfWeek.Friday,
                DayOfWeek.Saturday,
            }, gridMonth.DaysOfWeek);
        }
    }
}
