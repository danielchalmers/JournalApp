﻿namespace JournalApp;

public class DataPointCategory
{
    [Key]
    public Guid Guid { get; set; }

    public string Group { get; set; }

    public string Name { get; set; }

    public int Index { get; set; }

    public bool ReadOnly { get; set; }

    public bool Enabled { get; set; } = true;

    public DataType Type { get; set; }

    public decimal? MedicationDose { get; set; }

    public string MedicationUnit { get; set; }

    public bool MedicationEveryDay { get; set; }

    public virtual HashSet<DataPoint> DataPoints { get; set; } = new();

    public bool SingleLine => Type is DataType.Mood or DataType.Number;

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
    public decimal? MedicationDose { get; set; }
    public string MedicationUnit { get; set; }

    public override string ToString() => $"{DataType} ({Category})";

    public static IReadOnlyList<string> Moods { get; } = new[] { "🤔", "😁", "😀", "🙂", "😐", "🙁", "😢", "😭", };
}

public enum DataType
{
    [Description("Empty")]
    None,

    [Description("Mood emoji")]
    Mood,

    [Description("Sleep duration")]
    Sleep,

    [Description("1-5 scale")]
    Scale,

    [Description("Low to high")]
    LowToHigh,

    [Description("Mild to severe")]
    MildToSevere,

    [Description("Yes or no")]
    Bool,

    [Description("Decimal number")]
    Number,

    [Description("Short text")]
    Text,

    [Description("Long note")]
    Note,

    [Description("Medication")]
    Medication,
}