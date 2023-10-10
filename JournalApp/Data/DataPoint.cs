using System.Text;

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

    public DataType Type { get; set; }

    public decimal? MedicationDose { get; set; }

    public string MedicationUnit { get; set; }

    public DateTimeOffset? MedicationEveryDaySince { get; set; }

    public virtual HashSet<DataPoint> DataPoints { get; set; } = [];

    public bool SingleLine => Type is DataType.Mood or DataType.Number;

    public override string ToString()
    {
        var stringBuilder = new StringBuilder();

        if (Group != null)
        {
            stringBuilder.Append(Group);
            if (Name != null)
                stringBuilder.Append('|');
        }

        stringBuilder.Append(Name);

        stringBuilder.Append(',');
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

    public virtual Day Day { get; set; }

    public virtual DataPointCategory Category { get; set; }

    public bool IsDeleted { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DataType DataType { get; set; }

    public string Mood { get; set; }
    public decimal? SleepHours { get; set; }
    public int? ScaleIndex { get; set; }
    public bool? Bool { get; set; }
    public double? Number { get; set; }
    public string Text { get; set; }
    public decimal? MedicationDose { get; set; }
    public string MedicationUnit { get; set; }

    public override string ToString() => $"{DataType}, {Day}, {Category}";

    public static IReadOnlyList<string> Moods { get; } = new[] { "🤔", "🤩", "😀", "🙂", "😐", "😕", "😢", "😭", };

    public static DataPoint Create(Day day, DataPointCategory category)
    {
        return new()
        {
            Day = day,
            Category = category,
            CreatedAt = DateTimeOffset.Now,
            DataType = category.Type,
            MedicationDose = category.MedicationDose,
            MedicationUnit = category.MedicationUnit,
        };
    }
}

public enum DataType
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