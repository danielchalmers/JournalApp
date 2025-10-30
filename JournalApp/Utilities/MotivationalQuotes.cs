namespace JournalApp;

/// <summary>
/// Provides motivational quotes to display when users need encouragement.
/// </summary>
public static class MotivationalQuotes
{
    /// <summary>
    /// Collection of motivational quotes.
    /// </summary>
    private static readonly IReadOnlyList<string> Quotes =
    [
        "Take a moment to breathe",
        "You don't have to have all the answers",
        "Be gentle with yourself today",
        "It's okay to have tough days",
        "Let yourself take things slowly",
        "Tomorrow is a new opportunity",
        "Even small moments of peace matter",
        "This moment will not last forever",
        "It's okay to lean on others",
        "You're stronger than you think",
        "You are not alone in this moment",
        "One breath, one step at a time",
        "This feeling is temporary",
        "Every storm runs out of rain",
        "Be kind to yourself",
        "Small steps are still progress",
        "You are worthy of good things",
        "Tough times don't last",
        "Your feelings are valid",
        "Take it one moment at a time",
        "You are more resilient than you know",
        "You are not alone in this",
        "It's okay to ask for help when you need it",
        "One day at a time, one step at a time",
        "You are enough, just as you are",
        "Some days are for rest",
    ];

    /// <summary>
    /// Gets a random motivational quote.
    /// </summary>
    public static string GetRandomQuote() => Quotes[Random.Shared.Next(Quotes.Count)];
}
