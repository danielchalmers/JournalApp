using System.Globalization;

namespace JournalApp.Tests;

/// <summary>
/// GridMonth lays out a month as up-to-six week rows. The leading-blank offset formula
/// (GridMonth.cs) is the app's real calendar math and is only touched indirectly today.
/// Cultures are cloned with an explicit FirstDayOfWeek so the tests don't depend on host ICU data.
/// </summary>
public class GridMonthTests
{
    private static CultureInfo CultureStartingOn(DayOfWeek firstDay)
    {
        var culture = (CultureInfo)CultureInfo.InvariantCulture.Clone();
        culture.DateTimeFormat.FirstDayOfWeek = firstDay;
        return culture;
    }

    [Theory]
    [InlineData(DayOfWeek.Sunday, 1)] // 1 Jan 2024 is a Monday, so a Sunday-first week has one leading blank.
    [InlineData(DayOfWeek.Monday, 0)] // A Monday-first week starts the month in the first cell.
    public void GridDays_PlaceFirstOfMonthAfterCorrectLeadingBlanks(DayOfWeek firstDayOfWeek, int expectedBlanks)
    {
        var month = new GridMonth(2024, 1, CultureStartingOn(firstDayOfWeek), new());

        month.GridDays.TakeWhile(d => d.Date == null).Should().HaveCount(expectedBlanks);
        month.GridDays.First(d => d.Date != null).Date.Should().Be(new DateOnly(2024, 1, 1));
    }

    [Fact]
    public void GridDays_ContainEveryDayOfTheMonth()
    {
        // February 2024 is a leap February with 29 days.
        var month = new GridMonth(2024, 2, CultureStartingOn(DayOfWeek.Sunday), new());

        var realDates = month.GridDays.Where(d => d.Date != null).Select(d => d.Date!.Value).ToList();
        realDates.Should().HaveCount(29).And.OnlyHaveUniqueItems();
        realDates.Min().Should().Be(new DateOnly(2024, 2, 1));
        realDates.Max().Should().Be(new DateOnly(2024, 2, 29));
    }

    [Fact]
    public void GridDay_CarriesMoodEmoji_ForMatchingDate()
    {
        var date = new DateOnly(2024, 1, 15);
        var moodPoints = new Dictionary<DateOnly, DataPoint> { [date] = new() { Mood = "😀" } };

        var month = new GridMonth(2024, 1, CultureStartingOn(DayOfWeek.Sunday), moodPoints);

        month.GridDays.Single(d => d.Date == date).Emoji.Should().Be("😀");
        month.GridDays.Where(d => d.Date != null && d.Date != date).Should().OnlyContain(d => d.Emoji == null);
    }

    [Fact]
    public void DaysOfWeek_StartWithCultureFirstDay()
    {
        var month = new GridMonth(2024, 1, CultureStartingOn(DayOfWeek.Monday), new());

        month.DaysOfWeek.Should().HaveCount(7);
        month.DaysOfWeek.First().Should().Be(DayOfWeek.Monday);
        month.DaysOfWeek.Last().Should().Be(DayOfWeek.Sunday);
    }
}
