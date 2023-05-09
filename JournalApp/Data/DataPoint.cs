namespace JournalApp;

public class DataPoint
{
    [Key]
    public virtual int Id { get; set; }

    public virtual Day Day { get; set; }

    public virtual string Name { get; set; }

    public override string ToString() => $"{Name} ({Id})";
}

public class SleepDataPoint : DataPoint
{
    public virtual decimal? Hours { get; set; }
}

public class ScaleDataPoint : DataPoint
{
    public virtual int ScaleIndex { get; set; }
}

public class BoolDataPoint : DataPoint
{
    public virtual bool? Value { get; set; }
}

public class NumberDataPoint : DataPoint
{
    public virtual double? Value { get; set; }
}

public class TextDataPoint : DataPoint
{
    public virtual string Value { get; set; }
}

public class NoteDataPoint : TextDataPoint
{
}