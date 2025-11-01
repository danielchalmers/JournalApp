namespace JournalApp.Data;

/// <summary>
/// Helper methods for handling medication data point operations.
/// </summary>
public static class MedicationHelpers
{
    /// <summary>
    /// Updates the medication dose when the "taken" status changes.
    /// If the medication wasn't taken, resets the dose to the category's default dose.
    /// This allows users to easily reset custom doses by toggling the button twice.
    /// </summary>
    /// <param name="point">The medication data point to update.</param>
    public static void HandleMedicationTakenChanged(DataPoint point)
    {
        if (point == null)
            throw new ArgumentNullException(nameof(point));

        if (point.Category?.Type != PointType.Medication)
            throw new ArgumentException("DataPoint must be a medication type.", nameof(point));

        // If the medication wasn't taken, reset to category's default dose
        if (point.Bool != true)
        {
            point.MedicationDose = point.Category.MedicationDose;
        }
    }
}
