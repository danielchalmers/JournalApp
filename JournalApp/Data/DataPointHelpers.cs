namespace JournalApp.Data;

/// <summary>
/// Helper methods for handling data point value manipulations.
/// Provides a centralized layer for all data point property changes.
/// </summary>
public static class DataPointHelpers
{
    /// <summary>
    /// Decrements the sleep hours by 0.5, with a minimum of 0.
    /// </summary>
    /// <param name="point">The data point to update.</param>
    public static void DecrementSleep(DataPoint point)
    {
        if (point == null)
            throw new ArgumentNullException(nameof(point));

        if (point.Type != PointType.Sleep)
            throw new ArgumentException("DataPoint must be a sleep type.", nameof(point));

        point.SleepHours = Math.Max(0m, (point.SleepHours ?? 0) - 0.5m);
    }

    /// <summary>
    /// Increments the sleep hours by 0.5, with a maximum of 24.
    /// </summary>
    /// <param name="point">The data point to update.</param>
    public static void IncrementSleep(DataPoint point)
    {
        if (point == null)
            throw new ArgumentNullException(nameof(point));

        if (point.Type != PointType.Sleep)
            throw new ArgumentException("DataPoint must be a sleep type.", nameof(point));

        point.SleepHours = Math.Min(24m, (point.SleepHours ?? 0) + 0.5m);
    }

    /// <summary>
    /// Sets the mood value for a data point.
    /// </summary>
    /// <param name="point">The data point to update.</param>
    /// <param name="mood">The mood emoji to set.</param>
    public static void SetMood(DataPoint point, string mood)
    {
        if (point == null)
            throw new ArgumentNullException(nameof(point));

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
    public static void SetScaleIndex(DataPoint point, int value)
    {
        if (point == null)
            throw new ArgumentNullException(nameof(point));

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
    public static int GetScaleIndex(DataPoint point)
    {
        if (point == null)
            throw new ArgumentNullException(nameof(point));

        if (point.Type != PointType.Scale)
            throw new ArgumentException("DataPoint must be a scale type.", nameof(point));

        return point.ScaleIndex ?? 0;
    }
}
