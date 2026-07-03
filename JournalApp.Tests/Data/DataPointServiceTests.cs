using JournalApp.Data;

namespace JournalApp.Tests.Data;

/// <summary>
/// Tests for DataPointService — the centralized layer for data point value manipulations.
/// Each operation has at most one real branch (a 0/24 clamp, a 0-&gt;null conversion, a taken/not-taken
/// reset) plus type/null guards, so the value cases are exercised as [Theory] rows.
/// </summary>
public class DataPointServiceTests
{
    private readonly DataPointService _service = new();

    private static DataPoint PointOfType(PointType type) =>
        DataPoint.Create(Day.Create(new DateOnly(2024, 1, 1)), new DataPointCategory { Type = type });

    private static DataPoint SleepPoint(double? hours = null)
    {
        var point = PointOfType(PointType.Sleep);
        point.SleepHours = (decimal?)hours;
        return point;
    }

    private static DataPoint MedicationPoint(decimal? categoryDose) =>
        DataPoint.Create(Day.Create(new DateOnly(2024, 1, 1)), new DataPointCategory { Type = PointType.Medication, MedicationDose = categoryDose });

    [Theory]
    [InlineData(8.0, 7.5)]
    [InlineData(0.0, 0.0)]    // already at the floor
    [InlineData(null, 0.0)]   // null is treated as 0
    [InlineData(-5.0, 0.0)]   // never goes below zero
    public void DecrementSleep_StepsDownByHalfHourAndClampsAtZero(double? start, double expected)
    {
        var point = SleepPoint(start);

        _service.DecrementSleep(point);

        point.SleepHours.Should().Be((decimal)expected);
    }

    [Theory]
    [InlineData(8.0, 8.5)]
    [InlineData(24.0, 24.0)]   // already at the ceiling
    [InlineData(23.75, 24.0)]  // a half-hour step would overshoot, so it clamps
    [InlineData(null, 0.5)]    // null is treated as 0
    [InlineData(-5.0, -4.5)]   // increment has no lower clamp
    public void IncrementSleep_StepsUpByHalfHourAndClampsAt24(double? start, double expected)
    {
        var point = SleepPoint(start);

        _service.IncrementSleep(point);

        point.SleepHours.Should().Be((decimal)expected);
    }

    [Theory]
    [InlineData("😀")]
    [InlineData(null)]
    [InlineData("")]
    public void SetMood_StoresValue(string mood)
    {
        var point = PointOfType(PointType.Mood);

        _service.SetMood(point, mood);

        point.Mood.Should().Be(mood);
    }

    [Theory]
    [InlineData(3, 3)]
    [InlineData(0, null)]         // 0 represents "no rating"
    [InlineData(-5, -5)]          // no range validation
    [InlineData(999999, 999999)]
    public void SetScaleIndex_StoresValueButConvertsZeroToNull(int value, int? expected)
    {
        var point = PointOfType(PointType.Scale);

        _service.SetScaleIndex(point, value);

        point.ScaleIndex.Should().Be(expected);
    }

    [Theory]
    [InlineData(4, 4)]
    [InlineData(null, 0)] // null represents "no rating"
    public void GetScaleIndex_ReturnsValueOrZeroForNull(int? stored, int expected)
    {
        var point = PointOfType(PointType.Scale);
        point.ScaleIndex = stored;

        _service.GetScaleIndex(point).Should().Be(expected);
    }

    [Theory]
    [InlineData(false, 100.0, 100.0)] // not taken -> reset to category's default dose
    [InlineData(null, 100.0, 100.0)]  // null also counts as not taken
    [InlineData(false, null, null)]   // null category dose -> reset to null
    [InlineData(false, 0.0, 0.0)]
    [InlineData(false, -50.0, -50.0)] // category dose is copied verbatim, even if nonsensical
    public void HandleMedicationTakenChanged_ResetsToCategoryDose_WhenNotTaken(bool? taken, double? categoryDose, double? expected)
    {
        var point = MedicationPoint((decimal?)categoryDose);
        point.Bool = taken;
        point.MedicationDose = 150m; // a custom dose that should be discarded

        _service.HandleMedicationTakenChanged(point);

        point.MedicationDose.Should().Be((decimal?)expected);
    }

    [Fact]
    public void HandleMedicationTakenChanged_PreservesCustomDose_WhenTaken()
    {
        var point = MedicationPoint(100m);
        point.Bool = true;
        point.MedicationDose = 150m;

        _service.HandleMedicationTakenChanged(point);

        point.MedicationDose.Should().Be(150m);
    }

    [Fact]
    public void HandleMedicationTakenChanged_ToggleTwice_ResetsCustomDoseToDefault()
    {
        // The "toggle the taken button twice to reset a custom dose" workflow.
        var point = MedicationPoint(100m);

        point.Bool = true;
        point.MedicationDose = 150m;
        _service.HandleMedicationTakenChanged(point);
        point.MedicationDose.Should().Be(150m); // taken keeps the custom dose

        point.Bool = false;
        _service.HandleMedicationTakenChanged(point);
        point.MedicationDose.Should().Be(100m); // not taken resets to default

        point.Bool = true;
        _service.HandleMedicationTakenChanged(point);
        point.MedicationDose.Should().Be(100m); // stays at the default
    }

    public static TheoryData<Action<DataPointService>> NullPointOperations() => new()
    {
        service => service.DecrementSleep(null!),
        service => service.IncrementSleep(null!),
        service => service.SetMood(null!, "😀"),
        service => service.SetScaleIndex(null!, 3),
        service => service.GetScaleIndex(null!),
        service => service.HandleMedicationTakenChanged(null!),
    };

    [Theory]
    [MemberData(nameof(NullPointOperations))]
    public void Operations_ThrowArgumentNullException_WhenPointIsNull(Action<DataPointService> operation)
    {
        var act = () => operation(_service);

        act.Should().Throw<ArgumentNullException>().WithParameterName("point");
    }

    [Fact]
    public void SleepOperations_ThrowArgumentException_WhenNotSleepType()
    {
        var point = PointOfType(PointType.Mood);

        ((Action)(() => _service.DecrementSleep(point))).Should().Throw<ArgumentException>()
            .WithParameterName("point").WithMessage("DataPoint must be a sleep type.*");
        ((Action)(() => _service.IncrementSleep(point))).Should().Throw<ArgumentException>()
            .WithParameterName("point").WithMessage("DataPoint must be a sleep type.*");
    }

    [Fact]
    public void SetMood_ThrowsArgumentException_WhenNotMoodType()
    {
        var point = PointOfType(PointType.Sleep);

        ((Action)(() => _service.SetMood(point, "😀"))).Should().Throw<ArgumentException>()
            .WithParameterName("point").WithMessage("DataPoint must be a mood type.*");
    }

    [Fact]
    public void ScaleOperations_ThrowArgumentException_WhenNotScaleType()
    {
        var point = PointOfType(PointType.Mood);

        ((Action)(() => _service.SetScaleIndex(point, 3))).Should().Throw<ArgumentException>()
            .WithParameterName("point").WithMessage("DataPoint must be a scale type.*");
        ((Action)(() => _service.GetScaleIndex(point))).Should().Throw<ArgumentException>()
            .WithParameterName("point").WithMessage("DataPoint must be a scale type.*");
    }

    [Fact]
    public void HandleMedicationTakenChanged_ThrowsArgumentException_WhenNotMedicationType()
    {
        var point = PointOfType(PointType.Bool);

        ((Action)(() => _service.HandleMedicationTakenChanged(point))).Should().Throw<ArgumentException>()
            .WithParameterName("point").WithMessage("DataPoint must be a medication type.*");
    }
}
