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
        var changesMade = false;
        var day = await Days.SingleOrDefaultAsync(x => x.Date == date);

        if (day == null)
        {
            day = Day.Create(date);
            Days.Add(day);
            changesMade = true;
        }

        if (AddMissingPoints(day))
            changesMade = true;

        if (changesMade)
            await SaveChangesAsync();

        return day;
    }

    public List<Day> GetOrCreateDays(IEnumerable<DateOnly> dates)
    {
        var days = new HashSet<Day>(Days);
        var categories = new HashSet<DataPointCategory>(Categories);
        var addedDays = new HashSet<Day>();
        var foundDays = new List<Day>();

        foreach (var date in dates)
        {
            var day = days.FirstOrDefault(x => x.Date == date);

            if (day == null)
            {
                day = Day.Create(date);
                addedDays.Add(day);
            }

            foundDays.Add(day);
        }

        Days.AddRange(addedDays);

        SaveChanges();

        return foundDays;
    }

    public bool AddMissingPoints(Day day) => AddMissingPoints(day, new HashSet<DataPointCategory>(Categories), null);

    public bool AddMissingPoints(Day day, HashSet<DataPointCategory> categories, Random random)
    {
        var newPoints = new HashSet<DataPoint>();

        foreach (var category in categories)
        {
            if (category.Group == "Notes")
            {
                // First-launch example note.
                if (category.Points.Count == 0)
                {
                    var note = CreateNote(day);
                    note.Text = "I just started using JournalApp! 😎";
                    newPoints.Add(note);
                }
            }
            else
            {
                // Create a new data point for this category if it doesn't have one already.
                if (!day.Points.Any(x => x.Category.Guid == category.Guid && x.Type == category.Type))
                {
                    var point = DataPoint.Create(day, category);

                    if (random != null)
                    {
                        point.Mood = DataPoint.Moods[random.Next(1, DataPoint.Moods.Count)];
                        point.SleepHours = random.Next(0, 49) / 2.0m;
                        point.ScaleIndex = random.Next(0, 6);
                        point.Bool = Convert.ToBoolean(random.Next(0, 2));
                        point.Number = random.Next(0, 1000);
                    }

                    // Automatically mark daily medications as taken.
                    if (category.Enabled && category.MedicationEveryDaySince != null && day.Date >= DateOnly.FromDateTime(category.MedicationEveryDaySince.Value.Date))
                        point.Bool = true;

                    // Add to the database.
                    newPoints.Add(point);
                }
            }
        }

        Points.AddRange(newPoints);

        SaveChanges();

        return newPoints.Count > 0;
    }

    public async Task AddCategory(DataPointCategory category)
    {
        // Set index to the end of the last category in the same group.
        if (category.Index == default)
        {
            var groupCategories = Categories.Where(x => x.Group == category.Group).ToHashSet();
            category.Index = groupCategories.Count > 0 ? groupCategories.Max(x => x.Index) + 1 : 1;
        }

        Categories.Add(category);

        await SaveChangesAsync();
    }

    public async Task MoveCategoryUp(DataPointCategory category)
    {
        // Ensure no conflicts.
        await FixCategoryIndexes();

        var replaced = Categories.SingleOrDefault(x => x.Group == category.Group && x.Index == category.Index - 1);

        if (replaced == null)
            return;

        replaced.Index++;
        category.Index--;

        await SaveChangesAsync();
    }

    /// <summary>
    /// Removes gaps or overlap between indexes.
    /// </summary>
    public async Task FixCategoryIndexes()
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

        await SaveChangesAsync();
    }

    public DataPoint CreateNote(Day day)
    {
        var notes = Categories.Single(x => x.Group == "Notes");
        return DataPoint.Create(day, notes);
    }
}