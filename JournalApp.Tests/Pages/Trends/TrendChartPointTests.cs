namespace JournalApp.Tests;

/// <summary>
/// TrendChartPoint maps a DataPoint onto the numeric values plotted on the trends charts.
/// The mood mapping is the highest-risk piece: ChartEmojis is a hand-maintained list whose order is intentionally reversed relative to <see cref="DataPoint.Moods"/> and drops the "🤔" entry, with nothing linking the two at compile time.
/// </summary>
public class TrendChartPointTests
{
    private static DataPoint MoodPoint(string mood)
    {
        var point = DataPoint.Create(Day.Create(new DateOnly(2024, 1, 1)), new DataPointCategory { Type = PointType.Mood });
        point.Mood = mood;
        return point;
    }

    private static DataPoint MedPoint(bool? taken, decimal? dose, decimal? categoryDose, string categoryUnit)
    {
        var category = new DataPointCategory { Type = PointType.Medication, MedicationDose = categoryDose, MedicationUnit = categoryUnit };
        var point = DataPoint.Create(Day.Create(new DateOnly(2024, 1, 1)), category);
        point.Bool = taken;
        point.MedicationDose = dose;
        return point;
    }

    [Theory]
    [InlineData("😭", 1)]
    [InlineData("😢", 2)]
    [InlineData("😕", 3)]
    [InlineData("😐", 4)]
    [InlineData("🙂", 5)]
    [InlineData("😀", 6)]
    [InlineData("🤩", 7)]
    public void Mood_MapsEachChartEmojiToOneBasedIndex(string emoji, int expected)
    {
        new TrendChartPoint(new DateOnly(2024, 1, 1), MoodPoint(emoji)).Mood.Should().Be(expected);
    }

    [Theory]
    [InlineData("🤔")] // deliberately excluded from the chart scale
    [InlineData("")]
    [InlineData("not-an-emoji")]
    public void Mood_ReturnsNull_ForThinkingUnmappedOrEmpty(string mood)
    {
        new TrendChartPoint(new DateOnly(2024, 1, 1), MoodPoint(mood)).Mood.Should().BeNull();
    }

    [Fact]
    public void Mood_ReturnsNull_WhenPointIsNull()
    {
        new TrendChartPoint(new DateOnly(2024, 1, 1), null).Mood.Should().BeNull();
    }

    [Fact]
    public void MedDose_ReturnsPointDose_WhenTakenAndCategoryHasDose()
    {
        new TrendChartPoint(new DateOnly(2024, 1, 1), MedPoint(taken: true, dose: 150m, categoryDose: 100m, categoryUnit: null))
            .MedDose.Should().Be(150m);
    }

    [Fact]
    public void MedDose_ReturnsPointDose_WhenTakenAndCategoryHasUnitButNoDose()
    {
        new TrendChartPoint(new DateOnly(2024, 1, 1), MedPoint(taken: true, dose: 50m, categoryDose: null, categoryUnit: "mg"))
            .MedDose.Should().Be(50m);
    }

    [Fact]
    public void MedDose_ReturnsOne_WhenTakenButCategoryHasNoDoseOrUnit()
    {
        // A plain "taken/not taken" medication with no dose or unit charts as a flat 1.
        new TrendChartPoint(new DateOnly(2024, 1, 1), MedPoint(taken: true, dose: 999m, categoryDose: null, categoryUnit: null))
            .MedDose.Should().Be(1m);
    }

    [Fact]
    public void MedDose_ReturnsZero_WhenNotTaken()
    {
        new TrendChartPoint(new DateOnly(2024, 1, 1), MedPoint(taken: false, dose: 100m, categoryDose: 100m, categoryUnit: "mg"))
            .MedDose.Should().Be(0m);
    }

    [Fact]
    public void MedDose_ReturnsNull_WhenTakenIsNull()
    {
        new TrendChartPoint(new DateOnly(2024, 1, 1), MedPoint(taken: null, dose: 100m, categoryDose: 100m, categoryUnit: "mg"))
            .MedDose.Should().BeNull();
    }

    [Fact]
    public void Bool_MapsTakenStateToOneZeroOrNull()
    {
        var day = Day.Create(new DateOnly(2024, 1, 1));
        var category = new DataPointCategory { Type = PointType.Bool };

        DataPoint WithBool(bool? value)
        {
            var p = DataPoint.Create(day, category);
            p.Bool = value;
            return p;
        }

        new TrendChartPoint(new DateOnly(2024, 1, 1), WithBool(true)).Bool.Should().Be(1m);
        new TrendChartPoint(new DateOnly(2024, 1, 1), WithBool(false)).Bool.Should().Be(0m);
        new TrendChartPoint(new DateOnly(2024, 1, 1), WithBool(null)).Bool.Should().BeNull();
    }

    [Fact]
    public void PassthroughValues_ReflectUnderlyingPoint()
    {
        var point = DataPoint.Create(Day.Create(new DateOnly(2024, 1, 1)), new DataPointCategory { Type = PointType.Sleep });
        point.SleepHours = 7.5m;
        point.ScaleIndex = 3;
        point.Number = 42d;

        var tcp = new TrendChartPoint(new DateOnly(2024, 1, 1), point);
        tcp.SleepHours.Should().Be(7.5m);
        tcp.Scale.Should().Be(3m);
        tcp.Number.Should().Be(42m);
    }
}
