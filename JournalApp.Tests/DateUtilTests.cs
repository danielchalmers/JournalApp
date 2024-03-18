namespace JournalApp.Tests;

public class DateUtilTests
{
    [Fact]
    public void DatesToTest()
    {
        var start = new DateOnly(2023, 09, 1);
        var end = new DateOnly(2023, 09, 4);
        var expected = new DateOnly[] { new(2023, 09, 1), new(2023, 09, 2), new(2023, 09, 3), new(2023, 09, 4), };

        Assert.Equal(expected, start.DatesTo(end));
    }

    [Fact]
    public void NextTest()
    {
        var originalDate = new DateOnly(2023, 01, 01);
        var nextDate = new DateOnly(2023, 01, 02);

        Assert.Equal(nextDate, originalDate.Next());
    }

    [Fact]
    public void PreviousTest()
    {
        var originalDate = new DateOnly(2023, 01, 02);
        var previousDate = new DateOnly(2023, 01, 01);

        Assert.Equal(previousDate, originalDate.Previous());
    }
}
