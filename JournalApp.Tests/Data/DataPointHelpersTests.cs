using JournalApp.Data;

namespace JournalApp.Tests.Data;

/// <summary>
/// Tests for DataPointHelpers class.
/// </summary>
public class DataPointHelpersTests
{
    [Fact]
    public void DecrementSleep_DecreasesByHalfHour()
    {
        // Arrange
        var category = new DataPointCategory { Type = PointType.Sleep };
        var day = Day.Create(new DateOnly(2024, 1, 1));
        var point = DataPoint.Create(day, category);
        point.SleepHours = 8.0m;

        // Act
        DataPointHelpers.DecrementSleep(point);

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
        DataPointHelpers.DecrementSleep(point);

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
        DataPointHelpers.DecrementSleep(point);

        // Assert
        point.SleepHours.Should().Be(0.0m);
    }

    [Fact]
    public void DecrementSleep_ThrowsException_WhenPointIsNull()
    {
        // Act & Assert
        var act = () => DataPointHelpers.DecrementSleep(null!);
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
        var act = () => DataPointHelpers.DecrementSleep(point);
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
        DataPointHelpers.IncrementSleep(point);

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
        DataPointHelpers.IncrementSleep(point);

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
        DataPointHelpers.IncrementSleep(point);

        // Assert
        point.SleepHours.Should().Be(0.5m);
    }

    [Fact]
    public void IncrementSleep_ThrowsException_WhenPointIsNull()
    {
        // Act & Assert
        var act = () => DataPointHelpers.IncrementSleep(null!);
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
        var act = () => DataPointHelpers.IncrementSleep(point);
        act.Should().Throw<ArgumentException>()
            .WithParameterName("point")
            .WithMessage("DataPoint must be a sleep type.*");
    }

    [Fact]
    public void SetMood_UpdatesMoodValue()
    {
        // Arrange
        var category = new DataPointCategory { Type = PointType.Mood };
        var day = Day.Create(new DateOnly(2024, 1, 1));
        var point = DataPoint.Create(day, category);

        // Act
        DataPointHelpers.SetMood(point, "😀");

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
        DataPointHelpers.SetMood(point, null);

        // Assert
        point.Mood.Should().BeNull();
    }

    [Fact]
    public void SetMood_ThrowsException_WhenPointIsNull()
    {
        // Act & Assert
        var act = () => DataPointHelpers.SetMood(null!, "😀");
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
        var act = () => DataPointHelpers.SetMood(point, "😀");
        act.Should().Throw<ArgumentException>()
            .WithParameterName("point")
            .WithMessage("DataPoint must be a mood type.*");
    }

    [Fact]
    public void SetScaleIndex_SetsValue()
    {
        // Arrange
        var category = new DataPointCategory { Type = PointType.Scale };
        var day = Day.Create(new DateOnly(2024, 1, 1));
        var point = DataPoint.Create(day, category);

        // Act
        DataPointHelpers.SetScaleIndex(point, 3);

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
        DataPointHelpers.SetScaleIndex(point, 0);

        // Assert
        point.ScaleIndex.Should().BeNull();
    }

    [Fact]
    public void SetScaleIndex_ThrowsException_WhenPointIsNull()
    {
        // Act & Assert
        var act = () => DataPointHelpers.SetScaleIndex(null!, 3);
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
        var act = () => DataPointHelpers.SetScaleIndex(point, 3);
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
        var result = DataPointHelpers.GetScaleIndex(point);

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
        var result = DataPointHelpers.GetScaleIndex(point);

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public void GetScaleIndex_ThrowsException_WhenPointIsNull()
    {
        // Act & Assert
        var act = () => DataPointHelpers.GetScaleIndex(null!);
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
        var act = () => DataPointHelpers.GetScaleIndex(point);
        act.Should().Throw<ArgumentException>()
            .WithParameterName("point")
            .WithMessage("DataPoint must be a scale type.*");
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
        DataPointHelpers.IncrementSleep(point); // 8.5
        DataPointHelpers.IncrementSleep(point); // 9.0
        DataPointHelpers.DecrementSleep(point); // 8.5

        // Assert
        point.SleepHours.Should().Be(8.5m);
    }

    [Fact]
    public void ScaleOperations_WorkTogether()
    {
        // Arrange
        var category = new DataPointCategory { Type = PointType.Scale };
        var day = Day.Create(new DateOnly(2024, 1, 1));
        var point = DataPoint.Create(day, category);

        // Act - Set and get
        DataPointHelpers.SetScaleIndex(point, 5);
        var result1 = DataPointHelpers.GetScaleIndex(point);
        
        DataPointHelpers.SetScaleIndex(point, 0);
        var result2 = DataPointHelpers.GetScaleIndex(point);

        // Assert
        result1.Should().Be(5);
        result2.Should().Be(0);
        point.ScaleIndex.Should().BeNull();
    }
}
