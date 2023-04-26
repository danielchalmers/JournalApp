namespace JournalApp;

public class JournalEntry
{
    [Key]
    public DateTime? Date { get; set; }

    public bool IsDeleted { get; set; }
}
