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

    protected override void OnConfiguring(DbContextOptionsBuilder builder)
    {
        if (!builder.IsConfigured)
        {
            // Uncomment to use EF Core tooling.
            //builder.UseSqlite($"Data Source = temp");
        }

        base.OnConfiguring(builder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DataPointCategory>()
            .HasMany(e => e.Points)
            .WithOne(e => e.Category);

        modelBuilder.Entity<Day>()
            .HasMany(e => e.Points)
            .WithOne(e => e.Day);
    }

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
    /// Gets missing data points for the specified day and category.
    /// </summary>
    public HashSet<DataPoint> GetMissingPoints(Day day, DataPointCategory category, Random random)
    {
        var newPoints = new HashSet<DataPoint>();

        // Skip disabled categories to save database space.
        if (!category.Enabled || category.Deleted)
            return newPoints;

        if (category.Group == "Notes")
        {
            if (random == null && category.Points.Count == 0)
            {
                // First-launch example note.
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
            // Create a new data point for this category if it doesn't have one already.
            var point = DataPoint.Create(day, category);

            // Randomize values.
            if (random != null)
            {
                point.CreatedAt = new DateTime(day.Date, TimeOnly.FromTimeSpan(TimeSpan.FromMinutes(random.Next(1, 1440))), DateTimeKind.Local);
                point.Mood = DataPoint.Moods[random.Next(1, DataPoint.Moods.Count)];
                point.SleepHours = random.Next(0, 49) / 2.0m;
                point.ScaleIndex = random.Next(0, 6);
                point.Bool = Convert.ToBoolean(random.Next(0, 2));
                point.Number = random.Next(0, 1000);
            }

            // Automatically mark "every day" medications as taken.
            if (category.MedicationEveryDaySince != null && day.Date >= DateOnly.FromDateTime(category.MedicationEveryDaySince.Value.Date))
                point.Bool = true;

            newPoints.Add(point);
        }

        return newPoints;
    }

    /// <summary>
    /// Adds a new category to the database.
    /// </summary>
    public void AddCategory(DataPointCategory category)
    {
        // Set index to the end of the last category in the same group.
        if (category.Index == default)
        {
            var highestIndex = Categories.Where(x => x.Group == category.Group).Select(x => x.Index).AsEnumerable().Prepend(0).Max();

            category.Index = highestIndex + 1;
        }

        Categories.Add(category);
    }

    /// <summary>
    /// Moves the specified category up in the order.
    /// </summary>
    public async Task MoveCategoryUp(DataPointCategory category)
    {
        // Ensure no conflicts.
        FixCategoryIndexes();

        var replaced = await Categories.SingleOrDefaultAsync(x => x.Group == category.Group && x.Index == category.Index - 1);

        // No categories above to take the place of.
        if (replaced == null)
            return;

        replaced.Index++;
        category.Index--;
    }

    /// <summary>
    /// Fixes the indexes of the categories to remove gaps or overlap.
    /// </summary>
    public void FixCategoryIndexes()
    {
        foreach (var g in Categories.GroupBy(x => x.Group))
        {
            var i = 0;
            foreach (var c in g.OrderBy(x => x.Index))
            {
                if (c.Deleted)
                    c.Index = 0;
                else
                    c.Index = ++i;
            }
        }
    }

    public DataPoint CreateNote(Day day)
    {
        var notes = Categories.Single(x => x.Guid == new Guid("BF394F35-2228-4933-BF38-AF5B1B97AEF7"));
        return DataPoint.Create(day, notes);
    }
}
