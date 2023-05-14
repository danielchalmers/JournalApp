namespace JournalApp;

public class Day
{
    [Key]
    public Guid Guid { get; set; }

    public DateOnly Date { get; set; }

    public override string ToString() => $"{Date}";
}