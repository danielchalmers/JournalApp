namespace JournalApp;

public static class Worksheets
{
    private static IReadOnlyList<Worksheet> _collection;

    public static IReadOnlyList<Worksheet> Collection => _collection ??= GetAllWorksheets().ToList();

    private static IEnumerable<Worksheet> GetAllWorksheets()
    {
        yield return new()
        {
            Title = "My sample worksheet",
            Uri = "https://www.iana.org/help/example-domains",
            SourceUri = "https://example.com",
            Category = "Samples",
        };

        yield return new()
        {
            Title = "My second sample worksheet",
            Uri = "https://www.iana.org/help/example-domains",
            SourceUri = "https://example.com",
            Category = "Samples",
        };

        yield return new()
        {
            Title = "A sample worksheet in a different category",
            Uri = "https://en.wikipedia.org/wiki/Domain_name",
            SourceUri = "https://en.wikipedia.org",
            Category = "Samples 2",
        };
    }
}
