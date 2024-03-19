using System.Text.Json.Serialization;

namespace JournalApp;

public class DataPoint
{
    [Key]
    public Guid Guid { get; set; }

    [JsonIgnore]
    public virtual Day Day { get; set; }

    [JsonIgnore]
    public virtual DataPointCategory Category { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public PointType Type { get; set; }

    public bool Deleted { get; set; }

    public string Mood { get; set; }
    public decimal? SleepHours { get; set; }
    public int? ScaleIndex { get; set; }
    public bool? Bool { get; set; }
    public double? Number { get; set; }
    public string Text { get; set; }
    public decimal? MedicationDose { get; set; }

    [JsonIgnore]
    public bool IsTimestampedNote => Category?.Group == "Notes";

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
