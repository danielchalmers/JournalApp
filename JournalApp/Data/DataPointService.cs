namespace JournalApp.Data;

/// <summary>
/// Service for handling data point value manipulations.
/// Provides a centralized layer for all data point property changes.
/// </summary>
public class DataPointService
{
    /// <summary>
    /// Decrements the sleep hours by 0.5, with a minimum of 0.
    /// </summary>
    /// <param name="point">The data point to update.</param>
    public void DecrementSleep(DataPoint point)
    {
        ArgumentNullException.ThrowIfNull(point);

        if (point.Type != PointType.Sleep)
            throw new ArgumentException("DataPoint must be a sleep type.", nameof(point));

        point.SleepHours = Math.Max(0m, (point.SleepHours ?? 0) - 0.5m);
    }

    /// <summary>
    /// Increments the sleep hours by 0.5, with a maximum of 24.
    /// </summary>
    /// <param name="point">The data point to update.</param>
    public void IncrementSleep(DataPoint point)
    {
        ArgumentNullException.ThrowIfNull(point);

        if (point.Type != PointType.Sleep)
            throw new ArgumentException("DataPoint must be a sleep type.", nameof(point));

        point.SleepHours = Math.Min(24m, (point.SleepHours ?? 0) + 0.5m);
    }

    /// <summary>
    /// Sets the mood value for a data point.
    /// </summary>
    /// <param name="point">The data point to update.</param>
    /// <param name="mood">The mood emoji to set.</param>
    public void SetMood(DataPoint point, string mood)
    {
        ArgumentNullException.ThrowIfNull(point);

        if (point.Type != PointType.Mood)
            throw new ArgumentException("DataPoint must be a mood type.", nameof(point));

        point.Mood = mood;
    }

    /// <summary>
    /// Sets the scale index for rating-based data points.
    /// Converts 0 to null to represent "no rating".
    /// </summary>
    /// <param name="point">The data point to update.</param>
    /// <param name="value">The scale index value (0 will be converted to null).</param>
    public void SetScaleIndex(DataPoint point, int value)
    {
        ArgumentNullException.ThrowIfNull(point);

        if (point.Type != PointType.Scale)
            throw new ArgumentException("DataPoint must be a scale type.", nameof(point));

        point.ScaleIndex = value == 0 ? null : value;
    }

    /// <summary>
    /// Gets the scale index for rating-based data points.
    /// Converts null to 0 to represent "no rating".
    /// </summary>
    /// <param name="point">The data point to read.</param>
    /// <returns>The scale index value (null will be converted to 0).</returns>
    public int GetScaleIndex(DataPoint point)
    {
        ArgumentNullException.ThrowIfNull(point);

        if (point.Type != PointType.Scale)
            throw new ArgumentException("DataPoint must be a scale type.", nameof(point));

        return point.ScaleIndex ?? 0;
    }

    /// <summary>
    /// Updates the medication dose when the "taken" status changes.
    /// If the medication wasn't taken, resets the dose to the category's default dose.
    /// This allows users to easily reset custom doses by toggling the button twice.
    /// </summary>
    /// <param name="point">The medication data point to update.</param>
    public void HandleMedicationTakenChanged(DataPoint point)
    {
        ArgumentNullException.ThrowIfNull(point);

        if (point.Category?.Type != PointType.Medication)
            throw new ArgumentException("DataPoint must be a medication type.", nameof(point));

        // If the medication wasn't taken, reset to category's default dose
        if (point.Bool != true)
        {
            point.MedicationDose = point.Category.MedicationDose;
        }
    }
}
