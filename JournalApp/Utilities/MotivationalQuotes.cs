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
        "Remember: this too shall pass.",
        "You're doing better than you realize.",
        "It's okay to not be okay right now.",
        "Take it one moment at a time.",
        "You don't have to be perfect to be amazing.",
        "Your best is always good enough.",
        "Rest is not giving up. It's recharging.",
        "You've survived 100% of your worst days so far.",
        "Progress, not perfection.",
        "You are more resilient than you know.",
        "Healing is not linear, and that's okay.",
        "You deserve compassion, especially from yourself.",
        "Even the darkest night will end and the sun will rise.",
        "You are not alone in this.",
        "It's okay to ask for help when you need it.",
        "One day at a time, one step at a time.",
        "You are enough, just as you are.",
        "Storms make trees take deeper roots.",
        "Your journey is your own. Take your time.",
        "Breathe. You've got this moment, and that's enough.",
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
