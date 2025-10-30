namespace JournalApp;

/// <summary>
/// Service for calculating streaks based on mood entries.
/// </summary>
public class StreakService
{
    /// <summary>
    /// Calculates streak information for a collection of dates with mood points.
    /// </summary>
    /// <param name="moodPoints">Dictionary of dates with mood data points</param>
    /// <returns>Dictionary mapping dates to their streak information</returns>
    public static Dictionary<DateOnly, StreakInfo> CalculateStreaks(Dictionary<DateOnly, DataPoint> moodPoints)
    {
        var streakInfo = new Dictionary<DateOnly, StreakInfo>();
        
        if (moodPoints.Count == 0)
            return streakInfo;

        var sortedDates = moodPoints.Keys.OrderBy(d => d).ToList();
        
        for (int i = 0; i < sortedDates.Count; i++)
        {
            var currentDate = sortedDates[i];
            var isStreakStart = i == 0 || sortedDates[i - 1] != currentDate.Previous();
            var isStreakEnd = i == sortedDates.Count - 1 || sortedDates[i + 1] != currentDate.Next();
            var hasNext = !isStreakEnd;
            
            streakInfo[currentDate] = new StreakInfo(isStreakStart, hasNext);
        }

        return streakInfo;
    }
}

/// <summary>
/// Represents streak information for a specific date.
/// </summary>
/// <param name="IsStreakStart">True if this date is the start of a streak</param>
/// <param name="HasNext">True if the streak continues to the next day</param>
public readonly record struct StreakInfo(bool IsStreakStart, bool HasNext);
