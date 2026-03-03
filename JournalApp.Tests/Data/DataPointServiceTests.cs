using JournalApp.Data;

namespace JournalApp.Tests.Data;

/// <summary>
/// Tests for DataPointService class.
/// </summary>
public class DataPointServiceTests
{
    private readonly DataPointService _service = new();

    #region Sleep Operations Tests

    [Theory]
    [InlineData(8.0, 7.5)]
    [InlineData(0.0, 0.0)]
    [InlineData(null, 0.0)]
    public void DecrementSleep_UpdatesSleepHoursWithinBounds(decimal? initialSleepHours, decimal expectedSleepHours)
    {
        var category = new DataPointCategory { Type = PointType.Sleep };
        var day = Day.Create(new DateOnly(2024, 1, 1));
        var point = DataPoint.Create(day, category);
        point.SleepHours = initialSleepHours;

        _service.DecrementSleep(point);

        point.SleepHours.Should().Be(expectedSleepHours);
    }

    [Fact]
    public void DecrementSleep_ThrowsException_WhenPointIsNull()
    {
        // Act & Assert
        var act = () => _service.DecrementSleep(null!);
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("point");
    }

    [Fact]
    public void DecrementSleep_ThrowsException_WhenNotSleepType()
    {
        // Arrange
        var category = new DataPointCategory { Type = PointType.Mood };
        var day = Day.Create(new DateOnly(2024, 1, 1));
        var point = DataPoint.Create(day, category);

        // Act & Assert
        var act = () => _service.DecrementSleep(point);
        act.Should().Throw<ArgumentException>()
            .WithParameterName("point")
            .WithMessage("DataPoint must be a sleep type.*");
    }

    [Theory]
    [InlineData(8.0, 8.5)]
    [InlineData(24.0, 24.0)]
    [InlineData(null, 0.5)]
    public void IncrementSleep_UpdatesSleepHoursWithinBounds(decimal? initialSleepHours, decimal expectedSleepHours)
    {
        var category = new DataPointCategory { Type = PointType.Sleep };
        var day = Day.Create(new DateOnly(2024, 1, 1));
        var point = DataPoint.Create(day, category);
        point.SleepHours = initialSleepHours;

        _service.IncrementSleep(point);

        point.SleepHours.Should().Be(expectedSleepHours);
    }

    [Fact]
    public void IncrementSleep_ThrowsException_WhenPointIsNull()
    {
        // Act & Assert
        var act = () => _service.IncrementSleep(null!);
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("point");
    }

    [Fact]
    public void IncrementSleep_ThrowsException_WhenNotSleepType()
    {
        // Arrange
        var category = new DataPointCategory { Type = PointType.Mood };
        var day = Day.Create(new DateOnly(2024, 1, 1));
        var point = DataPoint.Create(day, category);

        // Act & Assert
        var act = () => _service.IncrementSleep(point);
        act.Should().Throw<ArgumentException>()
            .WithParameterName("point")
            .WithMessage("DataPoint must be a sleep type.*");
    }

    [Fact]
    public void SleepOperations_WorkTogether()
    {
        // Arrange
        var category = new DataPointCategory { Type = PointType.Sleep };
        var day = Day.Create(new DateOnly(2024, 1, 1));
        var point = DataPoint.Create(day, category);
        point.SleepHours = 8.0m;

        // Act - Multiple operations
        _service.IncrementSleep(point); // 8.5
        _service.IncrementSleep(point); // 9.0
        _service.DecrementSleep(point); // 8.5

        // Assert
        point.SleepHours.Should().Be(8.5m);
    }

    #endregion

    #region Mood Operations Tests

    [Theory]
    [InlineData("😀")]
    [InlineData("")]
    [InlineData(null)]
    public void SetMood_UpdatesMoodValue(string mood)
    {
        var category = new DataPointCategory { Type = PointType.Mood };
        var day = Day.Create(new DateOnly(2024, 1, 1));
        var point = DataPoint.Create(day, category);

        _service.SetMood(point, mood);

        point.Mood.Should().Be(mood);
    }

    [Fact]
    public void SetMood_ThrowsException_WhenPointIsNull()
    {
        // Act & Assert
        var act = () => _service.SetMood(null!, "😀");
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("point");
    }

    [Fact]
    public void SetMood_ThrowsException_WhenNotMoodType()
    {
        // Arrange
        var category = new DataPointCategory { Type = PointType.Sleep };
        var day = Day.Create(new DateOnly(2024, 1, 1));
        var point = DataPoint.Create(day, category);

        // Act & Assert
        var act = () => _service.SetMood(point, "😀");
        act.Should().Throw<ArgumentException>()
            .WithParameterName("point")
            .WithMessage("DataPoint must be a mood type.*");
    }

    #endregion

    #region Scale Operations Tests

    [Fact]
    public void SetScaleIndex_SetsValue()
    {
        // Arrange
        var category = new DataPointCategory { Type = PointType.Scale };
        var day = Day.Create(new DateOnly(2024, 1, 1));
        var point = DataPoint.Create(day, category);

        // Act
        _service.SetScaleIndex(point, 3);

        // Assert
        point.ScaleIndex.Should().Be(3);
    }

    [Fact]
    public void SetScaleIndex_ConvertsZeroToNull()
    {
        // Arrange
        var category = new DataPointCategory { Type = PointType.Scale };
        var day = Day.Create(new DateOnly(2024, 1, 1));
        var point = DataPoint.Create(day, category);
        point.ScaleIndex = 5;

        // Act
        _service.SetScaleIndex(point, 0);

        // Assert
        point.ScaleIndex.Should().BeNull();
    }

    [Fact]
    public void SetScaleIndex_ThrowsException_WhenPointIsNull()
    {
        // Act & Assert
        var act = () => _service.SetScaleIndex(null!, 3);
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("point");
    }

    [Fact]
    public void SetScaleIndex_ThrowsException_WhenNotScaleType()
    {
        // Arrange
        var category = new DataPointCategory { Type = PointType.Mood };
        var day = Day.Create(new DateOnly(2024, 1, 1));
        var point = DataPoint.Create(day, category);

        // Act & Assert
        var act = () => _service.SetScaleIndex(point, 3);
        act.Should().Throw<ArgumentException>()
            .WithParameterName("point")
            .WithMessage("DataPoint must be a scale type.*");
    }

    [Theory]
    [InlineData(4, 4)]
    [InlineData(null, 0)]
    public void GetScaleIndex_ReturnsExpectedValue(int? storedScaleIndex, int expectedScaleIndex)
    {
        var category = new DataPointCategory { Type = PointType.Scale };
        var day = Day.Create(new DateOnly(2024, 1, 1));
        var point = DataPoint.Create(day, category);
        point.ScaleIndex = storedScaleIndex;

        var result = _service.GetScaleIndex(point);

        result.Should().Be(expectedScaleIndex);
    }

    [Fact]
    public void GetScaleIndex_ThrowsException_WhenPointIsNull()
    {
        // Act & Assert
        var act = () => _service.GetScaleIndex(null!);
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("point");
    }

    [Fact]
    public void GetScaleIndex_ThrowsException_WhenNotScaleType()
    {
        // Arrange
        var category = new DataPointCategory { Type = PointType.Mood };
        var day = Day.Create(new DateOnly(2024, 1, 1));
        var point = DataPoint.Create(day, category);

        // Act & Assert
        var act = () => _service.GetScaleIndex(point);
        act.Should().Throw<ArgumentException>()
            .WithParameterName("point")
            .WithMessage("DataPoint must be a scale type.*");
    }

    [Fact]
    public void ScaleOperations_WorkTogether()
    {
        // Arrange
        var category = new DataPointCategory { Type = PointType.Scale };
        var day = Day.Create(new DateOnly(2024, 1, 1));
        var point = DataPoint.Create(day, category);

        // Act - Set and get
        _service.SetScaleIndex(point, 5);
        var result1 = _service.GetScaleIndex(point);

        _service.SetScaleIndex(point, 0);
        var result2 = _service.GetScaleIndex(point);

        // Assert
        result1.Should().Be(5);
        result2.Should().Be(0);
        point.ScaleIndex.Should().BeNull();
    }

    #endregion

    #region Medication Operations Tests

    [Fact]
    public void HandleMedicationTakenChanged_ResetsDose_WhenNotTaken()
    {
        // Arrange
        var category = new DataPointCategory
        {
            Type = PointType.Medication,
            MedicationDose = 100m
        };
        var day = Day.Create(new DateOnly(2024, 1, 1));
        var point = DataPoint.Create(day, category);
        point.Bool = false;
        point.MedicationDose = 150m; // Custom dose

        // Act
        _service.HandleMedicationTakenChanged(point);

        // Assert
        point.MedicationDose.Should().Be(100m); // Reset to category default
    }

    [Fact]
    public void HandleMedicationTakenChanged_ResetsDose_WhenNullNotTaken()
    {
        // Arrange
        var category = new DataPointCategory
        {
            Type = PointType.Medication,
            MedicationDose = 100m
        };
        var day = Day.Create(new DateOnly(2024, 1, 1));
        var point = DataPoint.Create(day, category);
        point.Bool = null; // Not taken (null)
        point.MedicationDose = 150m; // Custom dose

        // Act
        _service.HandleMedicationTakenChanged(point);

        // Assert
        point.MedicationDose.Should().Be(100m); // Reset to category default
    }

    [Fact]
    public void HandleMedicationTakenChanged_PreservesDose_WhenTaken()
    {
        // Arrange
        var category = new DataPointCategory
        {
            Type = PointType.Medication,
            MedicationDose = 100m
        };
        var day = Day.Create(new DateOnly(2024, 1, 1));
        var point = DataPoint.Create(day, category);
        point.Bool = true;
        point.MedicationDose = 150m; // Custom dose

        // Act
        _service.HandleMedicationTakenChanged(point);

        // Assert
        point.MedicationDose.Should().Be(150m); // Preserve custom dose
    }

    [Fact]
    public void HandleMedicationTakenChanged_HandlesNullCategoryDose()
    {
        // Arrange
        var category = new DataPointCategory
        {
            Type = PointType.Medication,
            MedicationDose = null // No default dose
        };
        var day = Day.Create(new DateOnly(2024, 1, 1));
        var point = DataPoint.Create(day, category);
        point.Bool = false;
        point.MedicationDose = 150m; // Custom dose

        // Act
        _service.HandleMedicationTakenChanged(point);

        // Assert
        point.MedicationDose.Should().BeNull(); // Reset to null
    }

    [Fact]
    public void HandleMedicationTakenChanged_ThrowsException_WhenPointIsNull()
    {
        // Act & Assert
        var act = () => _service.HandleMedicationTakenChanged(null!);
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("point");
    }

    [Fact]
    public void HandleMedicationTakenChanged_ThrowsException_WhenNotMedicationType()
    {
        // Arrange
        var category = new DataPointCategory
        {
            Type = PointType.Bool // Not a medication
        };
        var day = Day.Create(new DateOnly(2024, 1, 1));
        var point = DataPoint.Create(day, category);

        // Act & Assert
        var act = () => _service.HandleMedicationTakenChanged(point);
        act.Should().Throw<ArgumentException>()
            .WithParameterName("point")
            .WithMessage("DataPoint must be a medication type.*");
    }

    [Fact]
    public void HandleMedicationTakenChanged_AllowsToggleTwiceToResetDose()
    {
        // This test demonstrates the "toggle twice to reset" feature mentioned in the comment

        // Arrange
        var category = new DataPointCategory
        {
            Type = PointType.Medication,
            MedicationDose = 100m
        };
        var day = Day.Create(new DateOnly(2024, 1, 1));
        var point = DataPoint.Create(day, category);

        // User takes medication with custom dose
        point.Bool = true;
        point.MedicationDose = 150m;
        _service.HandleMedicationTakenChanged(point);
        point.MedicationDose.Should().Be(150m); // Keeps custom dose

        // User toggles to "not taken"
        point.Bool = false;
        _service.HandleMedicationTakenChanged(point);
        point.MedicationDose.Should().Be(100m); // Resets to default

        // User toggles back to "taken"
        point.Bool = true;
        _service.HandleMedicationTakenChanged(point);
        point.MedicationDose.Should().Be(100m); // Now has default dose again
    }

    #endregion

    #region Edge Cases and Null Handling

    [Fact]
    public void DecrementSleep_WithNegativeValue_StopsAtZero()
    {
        // Arrange
        var category = new DataPointCategory { Type = PointType.Sleep };
        var day = Day.Create(new DateOnly(2024, 1, 1));
        var point = DataPoint.Create(day, category);
        point.SleepHours = -5m;

        // Act
        _service.DecrementSleep(point);

        // Assert - Math.Max ensures it doesn't go more negative
        point.SleepHours.Should().Be(0m);
    }

    [Fact]
    public void IncrementSleep_BeyondMaximum_StopsAt24()
    {
        // Arrange
        var category = new DataPointCategory { Type = PointType.Sleep };
        var day = Day.Create(new DateOnly(2024, 1, 1));
        var point = DataPoint.Create(day, category);
        point.SleepHours = 23.75m;

        // Act - Try to increment multiple times
        _service.IncrementSleep(point); // 24.0
        _service.IncrementSleep(point); // Should stay 24.0
        _service.IncrementSleep(point); // Should stay 24.0

        // Assert
        point.SleepHours.Should().Be(24.0m);
    }

    [Fact]
    public void HandleMedicationTakenChanged_WithZeroDose_HandlesCorrectly()
    {
        // Arrange
        var category = new DataPointCategory
        {
            Type = PointType.Medication,
            MedicationDose = 0m
        };
        var day = Day.Create(new DateOnly(2024, 1, 1));
        var point = DataPoint.Create(day, category);
        point.Bool = false;
        point.MedicationDose = 100m;

        // Act
        _service.HandleMedicationTakenChanged(point);

        // Assert - Should reset to category's zero dose
        point.MedicationDose.Should().Be(0m);
    }

    [Fact]
    public void HandleMedicationTakenChanged_WithNegativeDose_HandlesCorrectly()
    {
        // Arrange
        var category = new DataPointCategory
        {
            Type = PointType.Medication,
            MedicationDose = -50m // Invalid but testing behavior
        };
        var day = Day.Create(new DateOnly(2024, 1, 1));
        var point = DataPoint.Create(day, category);
        point.Bool = false;
        point.MedicationDose = 100m;

        // Act
        _service.HandleMedicationTakenChanged(point);

        // Assert - Should reset to category's dose, even if negative
        point.MedicationDose.Should().Be(-50m);
    }

    [Fact]
    public void SleepOperations_WithDecimalPrecision_MaintainPrecision()
    {
        // Arrange
        var category = new DataPointCategory { Type = PointType.Sleep };
        var day = Day.Create(new DateOnly(2024, 1, 1));
        var point = DataPoint.Create(day, category);
        point.SleepHours = 7.75m;

        // Act
        _service.IncrementSleep(point); // 8.25
        _service.DecrementSleep(point); // 7.75

        // Assert - Should maintain decimal precision
        point.SleepHours.Should().Be(7.75m);
    }

    #endregion
}
