namespace JournalApp;

public class DataPointCategory
{
    /// <summary>
    /// The unique identifier for the category.
    /// </summary>
    [Key]
    public Guid Guid { get; set; }

    /// <summary>
    /// The group to which the category belongs.
    /// </summary>
    public string Group { get; set; }

    /// <summary>
    /// The name of the category.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The index of the category.
    /// </summary>
    public int Index { get; set; }

    /// <summary>
    /// Indicates whether the category is read-only.
    /// </summary>
    public bool ReadOnly { get; set; }

    /// <summary>
    /// Indicates whether the category is enabled.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Indicates whether the category is deleted.
    /// </summary>
    public bool Deleted { get; set; }

    /// <summary>
    /// The type of data points in the category.
    /// </summary>
    public PointType Type { get; set; }

    /// <summary>
    /// The medication dose for the category, if applicable.
    /// </summary>
    public decimal? MedicationDose { get; set; }

    /// <summary>
    /// The unit of the medication dose, if applicable.
    /// </summary>
    public string MedicationUnit { get; set; }

    /// <summary>
    /// The date since the medication is taken every day, if applicable.
    /// </summary>
    public DateTimeOffset? MedicationEveryDaySince { get; set; }

    /// <summary>
    /// Additional details about the category.
    /// </summary>
    public string Details { get; set; }

    /// <summary>
    /// The collection of data points in the category.
    /// </summary>
    public virtual HashSet<DataPoint> Points { get; set; } = [];

    /// <summary>
    /// Indicates whether the category should be shown on a single line.
    /// </summary>
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
