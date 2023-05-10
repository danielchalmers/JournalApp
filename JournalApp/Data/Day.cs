namespace JournalApp;

public class Day
{
    [Key]
    public int Id { get; set; }

    public DateOnly Date { get; set; }

    public virtual ICollection<DataPoint> DataPoints { get; set; }

    public virtual ICollection<NoteDataPoint> Notes { get; set; }

    public override string ToString() => $"{Date} ({Id})";

    public void AddNewNote()
    {
        var lastNoteSequenceNumber = Notes.Count == 0 ? 0 : Notes.Max(x => x.SequenceNumber);

        Notes.Add(new()
        {
            Name = $"Note at {DateTimeOffset.Now:h:mm:ss tt}",
            SequenceNumber = lastNoteSequenceNumber + 1
        });
    }
}
