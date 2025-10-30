namespace JournalApp;

/// <summary>
/// Provides motivational quotes to display when users need encouragement.
/// </summary>
public static class MotivationalQuotes
{
    private static readonly Random _random = new();

    /// <summary>
    /// Collection of motivational quotes.
    /// </summary>
    private static readonly IReadOnlyList<string> Quotes =
    [
        "It's okay to have tough days. Tomorrow is a new opportunity.",
        "You're stronger than you think.",
        "This feeling is temporary. You've got this.",
        "Every storm runs out of rain. Keep going.",
        "You've overcome challenges before, and you'll overcome this too.",
        "Be kind to yourself. You're doing the best you can.",
        "Small steps forward are still progress.",
        "You are worthy of good things, even on hard days.",
        "Tough times don't last, but tough people do.",
        "Your feelings are valid, and it's okay to take time to heal.",
    ];

    /// <summary>
    /// Gets a random motivational quote.
    /// </summary>
    /// <returns>A random motivational quote.</returns>
    public static string GetRandomQuote()
    {
        return Quotes[_random.Next(Quotes.Count)];
    }
}
