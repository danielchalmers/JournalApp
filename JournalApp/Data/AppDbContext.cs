namespace JournalApp;

public class AppDbContext : DbContext
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Day> Days { get; set; } = default!;
    public DbSet<DataPointCategory> Categories { get; set; } = default!;
    public DbSet<DataPoint> Points { get; set; } = default!;

    /// <summary>
    /// Configures the database context. Useful for local tooling.
    /// </summary>
    protected override void OnConfiguring(DbContextOptionsBuilder builder)
    {
        if (!builder.IsConfigured)
        {
            // Uncomment to enable EF Core tooling (e.g., migrations)
            //builder.UseSqlite($"Data Source = temp");
        }

        base.OnConfiguring(builder);
    }

    /// <summary>
    /// Configures entity relationships for EF Core.
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DataPointCategory>()
            .HasMany(e => e.Points)
            .WithOne(e => e.Category);

        modelBuilder.Entity<Day>()
            .HasMany(e => e.Points)
            .WithOne(e => e.Day);
    }

    /// <summary>
    /// Retrieves or creates a <see cref="Day"/> and populates it with any missing data points.
    /// </summary>
    public async Task<Day> GetOrCreateDayAndAddPoints(DateOnly date)
    {
        var day = await Days.FirstOrDefaultAsync(x => x.Date == date);

        if (day == null)
        {
            day = Day.Create(date);
            Days.Add(day);
        }

        var random =
#if DEBUG
            Random.Shared;
#else
            (Random)null;
#endif

        foreach (var category in Categories)
            await Points.AddRangeAsync(GetMissingPoints(day, category, random));

        return day;
    }

    /// <summary>
    /// Returns any data points that are missing for a given day and category.
    /// Includes optional random data generation for debug/testing.
    /// </summary>
    public HashSet<DataPoint> GetMissingPoints(Day day, DataPointCategory category, Random random)
    {
        var newPoints = new HashSet<DataPoint>();

        // Skip disabled or deleted categories.
        if (!category.Enabled || category.Deleted)
            return newPoints;

        if (category.Group == "Notes")
        {
            // Add a default welcome note only if no notes exist and not running in random mode.
            if (random == null && category.Points.Count == 0)
            {
                var note = CreateNote(day);
                var sb = new StringBuilder();
                sb.AppendLine("I just started using Good Diary! 😎");
                sb.AppendLine();
                sb.AppendLine("Click the date to go to the calendar");
                sb.Append("Find more features in the top right menu");
                note.Text = sb.ToString();
                newPoints.Add(note);
            }
        }
        else if (!day.Points.Any(x => x.Category.Guid == category.Guid && x.Type == category.Type))
        {
            // Add a new data point only if one doesn't already exist for the day/category/type.
            var point = DataPoint.Create(day, category);

            // Optionally generate random values for test data.
            if (random != null)
            {
                point.CreatedAt = new DateTime(day.Date, TimeOnly.FromTimeSpan(TimeSpan.FromMinutes(random.Next(1, 1440))), DateTimeKind.Local);
                point.Mood = DataPoint.Moods[random.Next(1, DataPoint.Moods.Count)];
                point.SleepHours = random.Next(0, 49) / 2.0m;
                point.ScaleIndex = random.Next(0, 6);
                point.Bool = Convert.ToBoolean(random.Next(0, 2));
                point.Number = random.Next(0, 1000);
            }

            // Automatically mark medication as taken if it's a daily med and the date applies.
            if (category.MedicationEveryDaySince != null && day.Date >= DateOnly.FromDateTime(category.MedicationEveryDaySince.Value.Date))
                point.Bool = true;

            newPoints.Add(point);
        }

        return newPoints;
    }

    /// <summary>
    /// Adds a new category and sets its display index within the group.
    /// </summary>
    public void AddCategory(DataPointCategory category)
    {
        if (category.Index == default)
        {
            var highestIndex = Categories
                .Where(x => x.Group == category.Group)
                .Select(x => x.Index)
                .AsEnumerable()
                .Prepend(0)
                .Max();

            category.Index = highestIndex + 1;
        }

        Categories.Add(category);
    }

    /// <summary>
    /// Moves a category up one position within its group.
    /// </summary>
    public async Task MoveCategoryUp(DataPointCategory category)
    {
        // Fix any broken or missing index values before proceeding.
        FixCategoryIndexes();

        var replaced = await Categories.SingleOrDefaultAsync(x =>
            x.Group == category.Group && x.Index == category.Index - 1);

        // No category above to swap with.
        if (replaced == null)
            return;

        replaced.Index++;
        category.Index--;
    }

    /// <summary>
    /// Reorders category indexes to remove gaps or duplicates.
    /// </summary>
    public void FixCategoryIndexes()
    {
        foreach (var g in Categories.GroupBy(x => x.Group))
        {
            var i = 0;
            foreach (var c in g.OrderBy(x => x.Index))
            {
                c.Index = c.Deleted ? 0 : ++i;
            }
        }
    }

    /// <summary>
    /// Creates a default note data point for the given day.
    /// </summary>
    public DataPoint CreateNote(Day day)
    {
        var notes = Categories.Single(x => x.Guid == new Guid("BF394F35-2228-4933-BF38-AF5B1B97AEF7"));
        return DataPoint.Create(day, notes);
    }
}
