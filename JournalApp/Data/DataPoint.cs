using System.Text.Json.Serialization;

namespace JournalApp;

/// <summary>
/// Represents a single data point recorded in the journal.
/// </summary>
public class DataPoint
{
    /// <summary>
    /// The unique identifier for the data point.
    /// </summary>
    [Key]
    public Guid Guid { get; set; }

    /// <summary>
    /// The day to which the data point belongs.
    /// </summary>
    [JsonIgnore]
    public virtual Day Day { get; set; }

    /// <summary>
    /// The category of the data point.
    /// </summary>
    [JsonIgnore]
    public virtual DataPointCategory Category { get; set; }

    /// <summary>
    /// The creation timestamp of the data point.
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    /// The type of the data point.
    /// </summary>
    public PointType Type { get; set; }

    /// <summary>
    /// Indicates whether the data point is deleted.
    /// </summary>
    public bool Deleted { get; set; }

    /// <summary>
    /// The mood value of the data point, if applicable.
    /// </summary>
    public string Mood { get; set; }

    /// <summary>
    /// The sleep hours value of the data point, if applicable.
    /// </summary>
    public decimal? SleepHours { get; set; }

    /// <summary>
    /// The scale index value of the data point, if applicable.
    /// </summary>
    public int? ScaleIndex { get; set; }

    /// <summary>
    /// The boolean value of the data point, if applicable.
    /// </summary>
    public bool? Bool { get; set; }

    /// <summary>
    /// The numeric value of the data point, if applicable.
    /// </summary>
    public double? Number { get; set; }

    /// <summary>
    /// The text value of the data point, if applicable.
    /// </summary>
    public string Text { get; set; }

    /// <summary>
    /// The medication dose value of the data point, if applicable.
    /// </summary>
    public decimal? MedicationDose { get; set; }

    /// <summary>
    /// Indicates whether the data point is pinned.
    /// </summary>
    public bool IsPinned { get; set; }

    /// <summary>
    /// Indicates whether the data point is a timestamped note.
    /// </summary>
    [JsonIgnore]
    public bool IsTimestampedNote => Category?.Group == "Notes";

    /// <summary>
    /// The list of predefined moods.
    /// </summary>
    [JsonIgnore]
    public static IReadOnlyList<string> Moods { get; } = ["🤔", "🤩", "😀", "🙂", "😐", "😕", "😢", "😭"];

    public override string ToString() => $"{Type}, {Category}, {Day}";

    public static DataPoint Create(Day day, DataPointCategory category)
    {
        return new()
        {
            Day = day,
            Category = category,
            CreatedAt = DateTimeOffset.Now,
            Type = category.Type,
            MedicationDose = category.MedicationDose,
        };
    }
}
