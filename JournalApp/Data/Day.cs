namespace JournalApp;

public class Day
{
    [Key]
    public int Id { get; set; }

    public DateOnly Date { get; set; }

    public virtual ICollection<DataPoint> DataPoints { get; set; }

    public virtual ICollection<NoteDataPoint> Notes { get; set; }

    public override string ToString() => $"{Date} ({Id})";
}
