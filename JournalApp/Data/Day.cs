namespace JournalApp;

public class Day
{
    [Key]
    public virtual int Id { get; set; }

    public virtual DateOnly Date { get; set; }

    public virtual ICollection<DataPoint> DataPoints { get; set; }

    public virtual ICollection<NoteDataPoint> Notes { get; set; }

    public override string ToString() => $"{Date} ({Id})";
}
