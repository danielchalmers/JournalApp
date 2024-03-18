namespace JournalApp;

public class Day
{
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
