using Xunit;

namespace JournalApp.Tests;

public class StreakServiceTests
{
    [Fact]
    public void CalculateStreaks_EmptyDictionary_ReturnsEmpty()
    {
        var moodPoints = new Dictionary<DateOnly, DataPoint>();
        var result = StreakService.CalculateStreaks(moodPoints);
        
        Assert.Empty(result);
    }

    [Fact]
    public void CalculateStreaks_SingleDay_IsStreakStartWithoutNext()
    {
        var date = new DateOnly(2024, 1, 1);
        var moodPoints = new Dictionary<DateOnly, DataPoint>
        {
            { date, new DataPoint { Mood = "😀" } }
        };
        
        var result = StreakService.CalculateStreaks(moodPoints);
        
        Assert.Single(result);
        Assert.True(result[date].IsStreakStart);
        Assert.False(result[date].HasNext);
    }

    [Fact]
    public void CalculateStreaks_ConsecutiveDays_CorrectStreakInfo()
    {
        var date1 = new DateOnly(2024, 1, 1);
        var date2 = new DateOnly(2024, 1, 2);
        var date3 = new DateOnly(2024, 1, 3);
        var moodPoints = new Dictionary<DateOnly, DataPoint>
        {
            { date1, new DataPoint { Mood = "😀" } },
            { date2, new DataPoint { Mood = "🙂" } },
            { date3, new DataPoint { Mood = "😢" } }
        };
        
        var result = StreakService.CalculateStreaks(moodPoints);
        
        Assert.Equal(3, result.Count);
        
        // First day: is start, has next
        Assert.True(result[date1].IsStreakStart);
        Assert.True(result[date1].HasNext);
        
        // Middle day: not start, has next
        Assert.False(result[date2].IsStreakStart);
        Assert.True(result[date2].HasNext);
        
        // Last day: not start, no next
        Assert.False(result[date3].IsStreakStart);
        Assert.False(result[date3].HasNext);
    }

    [Fact]
    public void CalculateStreaks_NonConsecutiveDays_MultipleStreaks()
    {
        var date1 = new DateOnly(2024, 1, 1);
        var date2 = new DateOnly(2024, 1, 2);
        var date4 = new DateOnly(2024, 1, 4);
        var date5 = new DateOnly(2024, 1, 5);
        var moodPoints = new Dictionary<DateOnly, DataPoint>
        {
            { date1, new DataPoint { Mood = "😀" } },
            { date2, new DataPoint { Mood = "🙂" } },
            { date4, new DataPoint { Mood = "😢" } },
            { date5, new DataPoint { Mood = "😭" } }
        };
        
        var result = StreakService.CalculateStreaks(moodPoints);
        
        Assert.Equal(4, result.Count);
        
        // First streak
        Assert.True(result[date1].IsStreakStart);
        Assert.True(result[date1].HasNext);
        Assert.False(result[date2].IsStreakStart);
        Assert.False(result[date2].HasNext); // Streak ends here
        
        // Second streak
        Assert.True(result[date4].IsStreakStart);
        Assert.True(result[date4].HasNext);
        Assert.False(result[date5].IsStreakStart);
        Assert.False(result[date5].HasNext);
    }

    [Fact]
    public void CalculateStreaks_UnorderedDates_HandlesCorrectly()
    {
        var date1 = new DateOnly(2024, 1, 3);
        var date2 = new DateOnly(2024, 1, 1);
        var date3 = new DateOnly(2024, 1, 2);
        var moodPoints = new Dictionary<DateOnly, DataPoint>
        {
            { date1, new DataPoint { Mood = "😀" } },
            { date2, new DataPoint { Mood = "🙂" } },
            { date3, new DataPoint { Mood = "😢" } }
        };
        
        var result = StreakService.CalculateStreaks(moodPoints);
        
        Assert.Equal(3, result.Count);
        
        // Should be ordered correctly despite input order
        Assert.True(result[date2].IsStreakStart); // Jan 1
        Assert.True(result[date2].HasNext);
        Assert.False(result[date3].IsStreakStart); // Jan 2
        Assert.True(result[date3].HasNext);
        Assert.False(result[date1].IsStreakStart); // Jan 3
        Assert.False(result[date1].HasNext);
    }

    [Fact]
    public void CalculateStreaks_LongStreak_CorrectStartAndContinuation()
    {
        var startDate = new DateOnly(2024, 1, 1);
        var moodPoints = new Dictionary<DateOnly, DataPoint>();
        
        // Create a 10-day streak
        for (int i = 0; i < 10; i++)
        {
            moodPoints[startDate.AddDays(i)] = new DataPoint { Mood = "😀" };
        }
        
        var result = StreakService.CalculateStreaks(moodPoints);
        
        Assert.Equal(10, result.Count);
        
        // First day should be streak start
        Assert.True(result[startDate].IsStreakStart);
        Assert.True(result[startDate].HasNext);
        
        // Middle days should not be starts but should have next
        for (int i = 1; i < 9; i++)
        {
            var date = startDate.AddDays(i);
            Assert.False(result[date].IsStreakStart);
            Assert.True(result[date].HasNext);
        }
        
        // Last day should not be start and should not have next
        var lastDate = startDate.AddDays(9);
        Assert.False(result[lastDate].IsStreakStart);
        Assert.False(result[lastDate].HasNext);
    }
}
