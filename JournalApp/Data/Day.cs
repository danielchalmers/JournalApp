namespace JournalApp;

public class Day
{
    [Key]
    public Guid Guid { get; set; }

    public DateOnly Date { get; set; }

    public virtual HashSet<DataPoint> DataPoints { get; set; } = new();

    public override string ToString() => $"{Date}";
}