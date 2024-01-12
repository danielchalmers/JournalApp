namespace JournalApp;

internal record PointOnChart(int Day, DataPoint Point)
{
    private static readonly List<string> ChartEmojis = ["😭", "😢", "😕", "😐", "🙂", "😀", "🤩"];

    public decimal? Mood
    {
        get
        {
            var i = ChartEmojis.IndexOf(Point?.Mood);

            return i == -1 ? null : i + 1;
        }
    }

    public decimal? SleepHours => Point?.SleepHours;

    public decimal? Scale => Point?.ScaleIndex;

    public decimal? Number => (decimal?)Point?.Number;

    public decimal? MedDose
    {
        get
        {
            if (Point?.Bool == true)
                return Point.MedicationDose;
            else if (Point?.Bool == false)
                return 0;

            return null;
        }
    }

    public decimal? Bool
    {
        get
        {
            if (Point?.Bool == true)
                return 1;
            else if (Point?.Bool == false)
                return 0;

            return null;
        }
    }
}
