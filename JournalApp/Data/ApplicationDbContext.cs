namespace JournalApp;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
        Database.EnsureCreated();
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

    public Task<Day> GetDay(DateTime dateTime) => GetDay(DateOnly.FromDateTime(dateTime));

    public async Task<Day> GetDay(DateOnly date)
    {
        var day = await Days.SingleOrDefaultAsync(x => x.Date == date);

        if (day == null)
        {
            var newDay = NewDay(date);

            Days.Add(newDay);
            await SaveChangesAsync();

            return newDay;
        }

        return day;
    }

    public Task<Day> GetNextDay(Day day) => GetDay(day.Date.GetNextDate());

    public Task<Day> GetPreviousDay(Day day) => GetDay(day.Date.GetPreviousDate());

    private static Day NewDay(DateOnly date) => new()
    {
        Id = date.DayNumber,
        Date = date,
        DataPoints = new List<DataPoint>
        {
            new SleepDataPoint { Name = "Sleep", SequenceNumber = 1 },
            new ScaleDataPoint { Name = "Happiness", SequenceNumber = 2 },
            new ScaleDataPoint { Name = "Productivity", SequenceNumber = 3 },
            new BoolDataPoint { Name = "Updated JournalApp", SequenceNumber = 4 },
            new NumberDataPoint { Name = "Weight", SequenceNumber = 5 },
        },
        Notes = new List<NoteDataPoint>
        {
        }
    };
}