namespace JournalApp;

public class Day
{
    [Key]
    public DateOnly Date { get; set; }

    public ICollection<DataPoint> DataPoints { get; set; }
}
