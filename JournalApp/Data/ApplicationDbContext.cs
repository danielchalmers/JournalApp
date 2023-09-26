namespace JournalApp;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    protected DbSet<Day> Days { get; set; } = default!;

    public DbSet<DataPointCategory> Categories { get; set; } = default!;

    public DbSet<DataPoint> DataPoints { get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DataPointCategory>()
            .HasMany(e => e.DataPoints)
            .WithOne(e => e.Category);

        modelBuilder.Entity<DataPoint>()
            .HasOne(e => e.Category);

        modelBuilder.Entity<DataPoint>()
            .HasOne(e => e.Day);
    }

    public Task<Day> GetOrCreateToday() => GetOrCreateDay(DateOnly.FromDateTime(DateTime.Now));

    public Task<Day> GetOrCreateNextDay(Day day) => GetOrCreateDay(day.Date.Next());

    public Task<Day> GetOrCreatePreviousDay(Day day) => GetOrCreateDay(day.Date.Previous());

    public async Task<Day> GetOrCreateDay(DateOnly date, bool saveOnChange = true, Random random = null)
    {
        var shouldSave = false;
        var day = await Days.SingleOrDefaultAsync(x => x.Date == date);

        if (day == null)
        {
            day = new()
            {
                Date = date,
            };

            await Days.AddAsync(day);
            shouldSave = true;
        }

        if (AddMissingDataPoints(day, random))
            shouldSave = true;

        if (shouldSave && saveOnChange)
            await SaveChangesAsync();

        return day;
    }

    public bool AddMissingDataPoints(Day day, Random random = null)
    {
        var anyAdded = false;

        foreach (var category in Categories)
        {
            var dataPoints = category.DataPoints;

            if (category.Group == "Notes")
            {
                // First-launch example note.
                if (dataPoints.Count == 0)
                {
                    var note = CreateNote(day);
                    note.Text = "I just started using JournalApp! 😎";
                    dataPoints.Add(note);
                }
            }
            else if (!dataPoints.Where(x => x.Day == day).Any(x => x.Category.Guid == category.Guid))
            {
                var dataPoint = DataPoint.Create(day, category);

                if (random != null)
                {
                    dataPoint.Mood = DataPoint.Moods[random.Next(1, DataPoint.Moods.Count)];
                    dataPoint.SleepHours = random.Next(0, 49) / 2.0m;
                    dataPoint.ScaleIndex = random.Next(0, 6);
                    dataPoint.Bool = Convert.ToBoolean(random.Next(0, 2));
                    dataPoint.Number = random.Next(0, 1000);
                }

                // Automatically mark daily medications as taken.
                if (category.Enabled && category.MedicationEveryDaySince != null && day.Date >= DateOnly.FromDateTime(category.MedicationEveryDaySince.Value.Date))
                    dataPoint.Bool = true;

                // Add to the database.
                if (dataPoints.Add(dataPoint))
                    anyAdded = true;
            }
        }

        return anyAdded;
    }

    public DataPoint CreateNote(Day day)
    {
        var notes = Categories.Single(x => x.Group == "Notes");
        return DataPoint.Create(day, notes);
    }
}