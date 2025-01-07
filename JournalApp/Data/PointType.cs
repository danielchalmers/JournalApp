namespace JournalApp;

/// <summary>
/// Represents different types of data points that can be recorded in the journal.
/// </summary>
public enum PointType
{
    None,
    Mood,
    Sleep,
    Scale,
    LowToHigh,
    MildToSevere,
    Bool,
    Number,
    Text,
    Note,
    Medication,
}
