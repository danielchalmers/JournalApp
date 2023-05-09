namespace JournalApp;

public class DataPoint
{
    [Key]
    public int Id { get; set; }

    public virtual Day Day { get; set; }

    public string Name { get; set; }

    public override string ToString() => $"{Name} ({Id})";
}

public class SleepDataPoint : DataPoint
{
    public decimal? Hours { get; set; }
}

public class ScaleDataPoint : DataPoint
{
    public int ScaleIndex { get; set; }
}

public class BoolDataPoint : DataPoint
{
    public bool? Value { get; set; }
}

public class NumberDataPoint : DataPoint
{
    public double? Value { get; set; }
}

public class TextDataPoint : DataPoint
{
    public string Value { get; set; }
}

public class NoteDataPoint : TextDataPoint
{
}