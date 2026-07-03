namespace JournalApp.Tests.Data;

/// <summary>
/// Pure formatting/predicate logic on the data models that feeds layout and validation.
/// </summary>
public class ModelTests
{
    [Theory]
    [InlineData("Medications", "Vitamin D", 3, "Medications | Vitamin D #3")]
    [InlineData("Medications", null, 0, "Medications #0")]
    [InlineData("Medications", "", 0, "Medications #0")]
    [InlineData(null, "Overall mood", 1, "Overall mood #1")]
    [InlineData(null, null, 5, " #5")]
    public void DataPointCategory_ToString_FormatsByGroupAndNamePresence(string group, string name, int index, string expected)
    {
        new DataPointCategory { Group = group, Name = name, Index = index }.ToString().Should().Be(expected);
    }

    [Theory]
    [InlineData(PointType.Mood, true)]
    [InlineData(PointType.Sleep, true)]
    [InlineData(PointType.Scale, true)]
    [InlineData(PointType.Number, true)]
    [InlineData(PointType.LowToHigh, false)]
    [InlineData(PointType.MildToSevere, false)]
    [InlineData(PointType.Bool, false)]
    [InlineData(PointType.Text, false)]
    [InlineData(PointType.Note, false)]
    [InlineData(PointType.Medication, false)]
    [InlineData(PointType.None, false)]
    public void DataPointCategory_SingleLine_TrueOnlyForMoodSleepScaleNumber(PointType type, bool expected)
    {
        new DataPointCategory { Type = type }.SingleLine.Should().Be(expected);
    }

    [Theory]
    [InlineData("Notes", true)]
    [InlineData("Medications", false)]
    [InlineData("notes", false)] // case-sensitive
    public void DataPoint_IsTimestampedNote_TrueOnlyForNotesGroup(string group, bool expected)
    {
        var point = DataPoint.Create(Day.Create(new DateOnly(2024, 1, 1)), new DataPointCategory { Group = group });
        point.IsTimestampedNote.Should().Be(expected);
    }

    [Fact]
    public void DataPoint_IsTimestampedNote_False_WhenCategoryNull()
    {
        new DataPoint().IsTimestampedNote.Should().BeFalse();
    }
}
