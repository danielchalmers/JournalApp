using JournalApp.Data;

namespace JournalApp.Tests.Data;

/// <summary>
/// Tests for MedicationHelpers class.
/// </summary>
public class MedicationHelpersTests
{
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
        MedicationHelpers.HandleMedicationTakenChanged(point);

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
        MedicationHelpers.HandleMedicationTakenChanged(point);

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
        MedicationHelpers.HandleMedicationTakenChanged(point);

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
        MedicationHelpers.HandleMedicationTakenChanged(point);

        // Assert
        point.MedicationDose.Should().BeNull(); // Reset to null
    }

    [Fact]
    public void HandleMedicationTakenChanged_ThrowsException_WhenPointIsNull()
    {
        // Act & Assert
        var act = () => MedicationHelpers.HandleMedicationTakenChanged(null!);
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
        var act = () => MedicationHelpers.HandleMedicationTakenChanged(point);
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
        MedicationHelpers.HandleMedicationTakenChanged(point);
        point.MedicationDose.Should().Be(150m); // Keeps custom dose
        
        // User toggles to "not taken"
        point.Bool = false;
        MedicationHelpers.HandleMedicationTakenChanged(point);
        point.MedicationDose.Should().Be(100m); // Resets to default
        
        // User toggles back to "taken"
        point.Bool = true;
        MedicationHelpers.HandleMedicationTakenChanged(point);
        point.MedicationDose.Should().Be(100m); // Now has default dose again
    }
}
