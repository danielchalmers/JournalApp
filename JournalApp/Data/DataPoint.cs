namespace JournalApp;

public class DataPointCategory
{
    [Key]
    public int Id { get; set; }

    public virtual Day Day { get; set; }

    public string Name { get; set; }

    public Type Type { get; set; }

    public int SequenceNumber { get; set; }

    public bool Enabled { get; set; } = true;

    public Dictionary<Day, DataPoint> Pairs = new();

    public override string ToString() => $"{Name} ({Id})";
}

public class DataPoint
{
    [Key]
    public int Id { get; set; }

    public virtual Day Day { get; set; }
}

public class SleepDataPoint : DataPoint
{
    public decimal? Hours { get; set; }

    public override string ToString() => $"Sleep: {Hours}";
}

public class ScaleDataPoint : DataPoint
{
    public int ScaleIndex { get; set; }

    public override string ToString() => $"Scale: {ScaleIndex}";
}

public class BoolDataPoint : DataPoint
{
    public bool? Value { get; set; }

    public override string ToString() => $"Bool: {Value}";
}

public class NumberDataPoint : DataPoint
{
    public double? Value { get; set; }

    public override string ToString() => $"Number: {Value}";
}

public class TextDataPoint : DataPoint
{
    public string Value { get; set; }

    public override string ToString() => $"Text: {Value.Length} long";
}

public class NoteDataPoint : TextDataPoint
{
    public override string ToString() => $"Note: {Value.Length} long";
}