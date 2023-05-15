namespace JournalApp;

public class DataPointCategory
{
    [Key]
    public Guid Guid { get; set; }

    public string Group { get; set; }

    public string Name { get; set; }

    public int Index { get; set; }

    public bool Enabled { get; set; } = true;

    public DataType Type { get; set; }

    public virtual HashSet<DataPoint> DataPoints { get; set; } = new();

    public override string ToString() => $"{string.Join("|", Group, Name)} #{Index}";
}

public class DataPoint
{
    [Key]
    public Guid Guid { get; set; }

    public virtual Day Day { get; set; }

    public virtual DataPointCategory Category { get; set; }

    public bool IsDeleted { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DataType DataType { get; set; }

    public string Mood { get; set; }
    public decimal? SleepHours { get; set; }
    public int ScaleIndex { get; set; }
    public bool? Bool { get; set; }
    public double? Number { get; set; }
    public string Text { get; set; }

    public override string ToString() => $"{DataType} ({Category})";

    public static IReadOnlyList<string> Moods { get; } = new[] { "😁", "😀", "🙂", "😐", "🙁", "😢", "😭", };
}

[Flags]
public enum DataType
{
    None = 0,
    Mood = 1 << 0,
    Sleep = 1 << 1,
    Scale = 1 << 2,
    Bool = 1 << 3,
    Number = 1 << 4,
    Text = 1 << 5,
}