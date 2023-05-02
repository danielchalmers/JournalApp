namespace JournalApp;

public class DataPoint
{
    [Key]
    public string Name { get; set; }
}

public class NumberDataPoint : DataPoint
{
    public int Value { get; set; }
}

public class ScaleDataPoint : DataPoint
{
    public int ScaleIndex { get; set; }
}

public class BoolDataPoint : DataPoint
{
    public bool Value { get; set; }
}

public class NoteDataPoint : DataPoint
{
    public int Value { get; set; }
}