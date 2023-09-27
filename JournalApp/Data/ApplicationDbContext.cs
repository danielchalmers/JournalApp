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

    public async Task<Day> GetOrCreateDay(DateOnly date, bool saveChanges = true, Random random = null)
    {
        var changesMade = false;
        var day = await Days.SingleOrDefaultAsync(x => x.Date == date);

        if (day == null)
        {
            day = new()
            {
                Date = date,
            };

            Days.Add(day);
            changesMade = true;
        }

        if (AddMissingDataPoints(day, random))
            changesMade = true;

        if (changesMade && saveChanges)
            await SaveChangesAsync();

        return day;
    }

    public bool AddMissingDataPoints(Day day, Random random = null)
    {
        var dayPoints = DataPoints.Where(x => x.Day.Guid == day.Guid).ToHashSet();
        var newPoints = new HashSet<DataPoint>();

        foreach (var category in Categories)
        {
            if (category.Group == "Notes")
            {
                // First-launch example note.
                if (category.DataPoints.Count == 0)
                {
                    var note = CreateNote(day);
                    note.Text = "I just started using JournalApp! 😎";
                    newPoints.Add(note);
                }
            }
            else
            {
                // Create a new data point for this category if it doesn't have one already.
                if (!dayPoints.Any(x => x.Category.Guid == category.Guid))
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
                    newPoints.Add(dataPoint);
                }
            }
        }

        DataPoints.AddRange(newPoints);

        return newPoints.Count > 0;
    }

    public DataPoint CreateNote(Day day)
    {
        var notes = Categories.Single(x => x.Group == "Notes");
        return DataPoint.Create(day, notes);
    }
}