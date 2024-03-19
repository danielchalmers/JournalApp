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
