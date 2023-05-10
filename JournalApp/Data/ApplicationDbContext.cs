namespace JournalApp;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
        Database.EnsureDeleted();
        Database.EnsureCreated();

        DataPointCategories ??= new List<DataPointCategory>
        {
            new() { Name = "Sleep", Type = typeof(SleepDataPoint), SequenceNumber = 1 },
            new() { Name = "Happiness", Type = typeof(ScaleDataPoint), SequenceNumber = 2 },
            new() { Name = "Productivity", Type = typeof(ScaleDataPoint), SequenceNumber = 3 },
            new() { Name = "Updated JournalApp", Type = typeof(BoolDataPoint), SequenceNumber = 4 },
            new() { Name = "Weight", Type = typeof(NumberDataPoint), SequenceNumber = 5 },
        };
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Day>()
            .HasMany(e => e.DataPoints)
            .WithOne(e => e.Day);

        modelBuilder.Entity<SleepDataPoint>().HasBaseType<DataPoint>();
        modelBuilder.Entity<ScaleDataPoint>().HasBaseType<DataPoint>();
        modelBuilder.Entity<BoolDataPoint>().HasBaseType<DataPoint>();
        modelBuilder.Entity<NumberDataPoint>().HasBaseType<DataPoint>();
        modelBuilder.Entity<TextDataPoint>().HasBaseType<DataPoint>();
        modelBuilder.Entity<NoteDataPoint>().HasBaseType<TextDataPoint>();
    }

    protected DbSet<Day> Days { get; set; } = default!;

    public IList<DataPointCategory> DataPointCategories { get; set; }

    public Task<Day> GetDay(DateTime dateTime) => GetDay(DateOnly.FromDateTime(dateTime));

    public async Task<Day> GetDay(DateOnly date)
    {
        var day = await Days.SingleOrDefaultAsync(x => x.Date == date);

        if (day == null)
        {
            day = NewDay(date);
            Days.Add(day);
        }

        // Add missing data points.
        foreach (var template in DataPointCategories.Where(x => !day.DataPoints.Any(y => y.Name == x.Name)))
        {
            var dataPoint = (DataPoint)template.Clone();
            dataPoint.Id = default;
            dataPoint.Day = day;
            day.DataPoints.Add(dataPoint);
        }

        // Force IDs to be assigned.
        await SaveChangesAsync();

        return day;
    }

    public Task<Day> GetNextDay(Day day) => GetDay(day.Date.GetNextDate());

    public Task<Day> GetPreviousDay(Day day) => GetDay(day.Date.GetPreviousDate());

    private static Day NewDay(DateOnly date) => new()
    {
        Id = date.DayNumber,
        Date = date,
        DataPoints = new List<DataPoint>(),
        Notes = new List<NoteDataPoint>(),
    };
}