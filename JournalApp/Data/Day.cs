namespace JournalApp;

/// <summary>
/// Represents a single day in the journal, containing data points.
/// </summary>
public class Day
{
    /// <summary>
    /// The unique identifier for the day.
    /// </summary>
    [Key]
    public Guid Guid { get; set; }

    public DateOnly Date { get; set; }

    public virtual HashSet<DataPoint> Points { get; set; } = [];

    public override string ToString() => $"{Date}";

    public static Day Create(DateOnly date)
    {
        return new()
        {
            Date = date,
        };
    }
}
