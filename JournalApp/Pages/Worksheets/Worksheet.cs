namespace JournalApp;

public record class Worksheet
{
    public string Title { get; set; }

    public string Description { get; set; }

    public string SourceUri { get; set; }

    public string Category { get; set; }

    public override string ToString() => Title;
}
