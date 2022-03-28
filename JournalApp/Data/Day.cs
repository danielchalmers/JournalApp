using System.ComponentModel.DataAnnotations;

namespace JournalApp;

public class Day
{
    [Key]
    public DateOnly Date { get; set; }

    public string Text { get; set; }
}
