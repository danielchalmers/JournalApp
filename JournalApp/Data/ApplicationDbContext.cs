namespace JournalApp;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
        Database.EnsureCreated();

        // Default data point categories.
        if (!DataPointCategories.Any(x => x.Name == "Sleep"))
            DataPointCategories.Add(new() { Name = "Sleep", Type = DataType.Sleep, Index = 1 });
        if (!DataPointCategories.Any(x => x.Name == "Happiness"))
            DataPointCategories.Add(new() { Name = "Happiness", Type = DataType.Scale, Index = 2 });
        if (!DataPointCategories.Any(x => x.Name == "Productivity"))
            DataPointCategories.Add(new() { Name = "Productivity", Type = DataType.Scale, Index = 3 });
        if (!DataPointCategories.Any(x => x.Name == "Updated JournalApp"))
            DataPointCategories.Add(new() { Name = "Updated JournalApp", Type = DataType.Bool, Index = 4 });
        if (!DataPointCategories.Any(x => x.Name == "Weight"))
            DataPointCategories.Add(new() { Name = "Weight", Type = DataType.Number, Index = 5 });
        if (!DataPointCategories.Any(x => x.Name == "Note"))
            DataPointCategories.Add(new() { Group = "Notes", Name = "Note", Type = DataType.Text });
    }

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

    protected DbSet<Day> Days { get; set; } = default!;

    public DbSet<DataPointCategory> DataPointCategories { get; set; }

    public Task<Day> GetDay(DateTime dateTime) => GetDay(DateOnly.FromDateTime(dateTime));

    public async Task<Day> GetDay(DateOnly date)
    {
        var day = await Days.SingleOrDefaultAsync(x => x.Date == date);

        if (day == null)
        {
            day = NewDay(date);
            Days.Add(day);
        }

        // Add any missing data points.
        foreach (var category in DataPointCategories)
        {
            if (!category.DataPoints.Where(x => x.Day == day).Any(x => x.Category.Name == category.Name))
            {
                category.DataPoints.Add(new DataPoint
                {
                    Day = day,
                    Category = category,
                    DataType = category.Type,
                });
            }
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
    };
}