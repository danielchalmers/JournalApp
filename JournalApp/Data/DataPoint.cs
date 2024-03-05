using System.Text.Json.Serialization;

namespace JournalApp;

public class DataPointCategory
{
    [Key]
    public Guid Guid { get; set; }

    public string Group { get; set; }

    public string Name { get; set; }

    public int Index { get; set; }

    public bool ReadOnly { get; set; }

    public bool Enabled { get; set; } = true;

    public bool Deleted { get; set; }

    public PointType Type { get; set; }

    public decimal? MedicationDose { get; set; }

    public string MedicationUnit { get; set; }

    public DateTimeOffset? MedicationEveryDaySince { get; set; }

    public string Details { get; set; }

    public virtual HashSet<DataPoint> Points { get; set; } = [];

    public bool SingleLine => Type is PointType.Mood or PointType.Sleep or PointType.Scale or PointType.Number;

    public override string ToString()
    {
        var stringBuilder = new StringBuilder();

        if (!string.IsNullOrEmpty(Group))
        {
            stringBuilder.Append(Group);
            if (!string.IsNullOrEmpty(Name))
            {
                stringBuilder.Append(' ');
                stringBuilder.Append('|');
                stringBuilder.Append(' ');
            }
        }

        stringBuilder.Append(Name);

        stringBuilder.Append(' ');
        stringBuilder.Append('#');
        stringBuilder.Append(Index);

        return stringBuilder.ToString();
    }
}

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