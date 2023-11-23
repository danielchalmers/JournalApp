namespace JournalApp.Tests;

public class MoodGridTests
{
    [Fact]
    [Description("Are the number of years, and months in the grid correct?")]
    public void YearsAndMonths()
    {
        var year = 2023;
        var gridYear = new GridYear(year, CultureInfo.InvariantCulture, []);

        Assert.Equal(12, gridYear.GridMonths.Count);

        for (var i = 0; i < gridYear.GridMonths.Count; i++)
        {
            var gridMonth = gridYear.GridMonths[i];

            Assert.Equal(year, gridMonth.Year);
            Assert.Equal(i + 1, gridMonth.Month);
        }
    }

    [Fact]
    [Description("Are the number of days in the grid correct with no unnecessary rows?")]
    public void GridDays()
    {
        var year = 2023;
        var gridYear = new GridYear(year, CultureInfo.InvariantCulture, []);

        var rowCounts = new List<int>()
        {
            35, // January.
            35, // February.
            35, // March.
            42, // April.
            35, // May.
            35, // June.
            42, // July.
            35, // August.
            35, // September.
            35, // October.
            35, // November
            42, // December.
        };

        Assert.Equal(rowCounts, gridYear.GridMonths.Select(m => m.GridDays.Count));
    }

    [Fact]
    [Description("Are the days in the week as expected on an invariant culture?")]
    public void DaysOfWeekInvariant()
    {
        var gridYear = new GridYear(2023, CultureInfo.InvariantCulture, []);

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
