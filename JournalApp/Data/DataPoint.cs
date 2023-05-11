namespace JournalApp;

public class DataPointCategory
{
    [Key]
    public int Id { get; set; }

    public string Group { get; set; }

    public string Name { get; set; }

    public int SequenceNumber { get; set; }

    public bool Enabled { get; set; } = true;

    public DataType Type { get; set; }

    public virtual HashSet<DataPoint> DataPoints { get; set; } = new();

    public override string ToString() => $"{string.Join("|", Group, Name)} - #{SequenceNumber}";
}

public class DataPoint
{
    [Key]
    public int Id { get; set; }

    public virtual Day Day { get; set; }

    public virtual DataPointCategory Category { get; set; }

    public DataType DataType { get; set; }

    public decimal? SleepHours { get; set; }
    public int ScaleIndex { get; set; }
    public bool? Bool { get; set; }
    public double? Number { get; set; }
    public string Text { get; set; }

    public override string ToString() => $"{DataType} ({Category})";
}

[Flags]
public enum DataType
{
    None = 0,
    Sleep = 1 << 0,
    Scale = 1 << 1,
    Bool = 1 << 2,
    Number = 1 << 3,
    Text = 1 << 4,
}