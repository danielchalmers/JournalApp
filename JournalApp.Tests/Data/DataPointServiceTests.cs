using JournalApp.Data;

namespace JournalApp.Tests.Data;

/// <summary>
/// Tests for DataPointService class.
/// </summary>
public class DataPointServiceTests
{
    private readonly DataPointService _service = new();

    #region Sleep Operations Tests

    [Fact]
    public void DecrementSleep_DecreasesByHalfHour()
    {
        // Arrange
        var category = new DataPointCategory { Type = PointType.Sleep };
        var day = Day.Create(new DateOnly(2024, 1, 1));
        var point = DataPoint.Create(day, category);
        point.SleepHours = 8.0m;

        // Act
        _service.DecrementSleep(point);

        // Assert
        point.SleepHours.Should().Be(7.5m);
    }

    [Fact]
    public void DecrementSleep_StopsAtZero()
    {
        // Arrange
        var category = new DataPointCategory { Type = PointType.Sleep };
        var day = Day.Create(new DateOnly(2024, 1, 1));
        var point = DataPoint.Create(day, category);
        point.SleepHours = 0.0m;

        // Act
        _service.DecrementSleep(point);

        // Assert
        point.SleepHours.Should().Be(0.0m);
    }

    [Fact]
    public void DecrementSleep_HandlesNull()
    {
        // Arrange
        var category = new DataPointCategory { Type = PointType.Sleep };
        var day = Day.Create(new DateOnly(2024, 1, 1));
        var point = DataPoint.Create(day, category);
        point.SleepHours = null;

        // Act
        _service.DecrementSleep(point);

        // Assert
        point.SleepHours.Should().Be(0.0m);
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

    [Fact]
    public void IncrementSleep_IncreasesByHalfHour()
    {
        // Arrange
        var category = new DataPointCategory { Type = PointType.Sleep };
        var day = Day.Create(new DateOnly(2024, 1, 1));
        var point = DataPoint.Create(day, category);
        point.SleepHours = 8.0m;

        // Act
        _service.IncrementSleep(point);

        // Assert
        point.SleepHours.Should().Be(8.5m);
    }

    [Fact]
    public void IncrementSleep_StopsAt24()
    {
        // Arrange
        var category = new DataPointCategory { Type = PointType.Sleep };
        var day = Day.Create(new DateOnly(2024, 1, 1));
        var point = DataPoint.Create(day, category);
        point.SleepHours = 24.0m;

        // Act
        _service.IncrementSleep(point);

        // Assert
        point.SleepHours.Should().Be(24.0m);
    }

    [Fact]
    public void IncrementSleep_HandlesNull()
    {
        // Arrange
        var category = new DataPointCategory { Type = PointType.Sleep };
        var day = Day.Create(new DateOnly(2024, 1, 1));
        var point = DataPoint.Create(day, category);
        point.SleepHours = null;

        // Act
        _service.IncrementSleep(point);

        // Assert
        point.SleepHours.Should().Be(0.5m);
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

    [Fact]
    public void SetMood_UpdatesMoodValue()
    {
        // Arrange
        var category = new DataPointCategory { Type = PointType.Mood };
        var day = Day.Create(new DateOnly(2024, 1, 1));
        var point = DataPoint.Create(day, category);

        // Act
        _service.SetMood(point, "😀");

        // Assert
        point.Mood.Should().Be("😀");
    }

    [Fact]
    public void SetMood_AllowsNullValue()
    {
        // Arrange
        var category = new DataPointCategory { Type = PointType.Mood };
        var day = Day.Create(new DateOnly(2024, 1, 1));
        var point = DataPoint.Create(day, category);
        point.Mood = "😀";

        // Act
        _service.SetMood(point, null);

        // Assert
        point.Mood.Should().BeNull();
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

    [Fact]
    public void GetScaleIndex_ReturnsValue()
    {
        // Arrange
        var category = new DataPointCategory { Type = PointType.Scale };
        var day = Day.Create(new DateOnly(2024, 1, 1));
        var point = DataPoint.Create(day, category);
        point.ScaleIndex = 4;

        // Act
        var result = _service.GetScaleIndex(point);

        // Assert
        result.Should().Be(4);
    }

    [Fact]
    public void GetScaleIndex_ConvertsNullToZero()
    {
        // Arrange
        var category = new DataPointCategory { Type = PointType.Scale };
        var day = Day.Create(new DateOnly(2024, 1, 1));
        var point = DataPoint.Create(day, category);
        point.ScaleIndex = null;

        // Act
        var result = _service.GetScaleIndex(point);

        // Assert
        result.Should().Be(0);
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
}
