namespace JournalApp;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
        Database.EnsureCreated();

        // Seed categories.
        if (!DataPointCategories.Any(x => x.Guid == new Guid("D90D89FB-F5B9-47CF-AE4E-3EC0D635E783")))
        {
            DataPointCategories.Add(new()
            {
                Guid = new Guid("D90D89FB-F5B9-47CF-AE4E-3EC0D635E783"),
                Name = "Mood",
                Type = DataType.Mood,
                Index = 1,
            });
        }

        if (!DataPointCategories.Any(x => x.Guid == new Guid("D8657B36-F3A0-486F-BF80-0CF057919C7D")))
        {
            DataPointCategories.Add(new()
            {
                Guid = new Guid("D8657B36-F3A0-486F-BF80-0CF057919C7D"),
                Name = "Sleep",
                Type = DataType.Sleep,
                Index = 2,
            });
        }

        if (!DataPointCategories.Any(x => x.Guid == new Guid("74B8FFBF-2251-4029-A655-7D37F69F47CD")))
        {
            DataPointCategories.Add(new()
            {
                Guid = new Guid("74B8FFBF-2251-4029-A655-7D37F69F47CD"),
                Name = "Happiness",
                Type = DataType.Scale,
                Index = 3,
            });
        }

        if (!DataPointCategories.Any(x => x.Guid == new Guid("40B5AF7B-4F4E-4E77-BD6B-F7855CF773AB")))
        {
            DataPointCategories.Add(new()
            {
                Guid = new Guid("40B5AF7B-4F4E-4E77-BD6B-F7855CF773AB"),
                Name = "Productivity",
                Type = DataType.Scale,
                Index = 4,
            });
        }

        if (!DataPointCategories.Any(x => x.Guid == new Guid("B764A259-8BBC-4578-AE7B-83B2CFB92FF3")))
        {
            DataPointCategories.Add(new()
            {
                Guid = new Guid("B764A259-8BBC-4578-AE7B-83B2CFB92FF3"),
                Name = "Updated JournalApp",
                Type = DataType.Bool,
                Index = 5,
            });
        }

        if (!DataPointCategories.Any(x => x.Guid == new Guid("480DC07D-1330-486F-9B30-EC83A3D4E6F0")))
        {
            DataPointCategories.Add(new()
            {
                Guid = new Guid("480DC07D-1330-486F-9B30-EC83A3D4E6F0"),
                Name = "Weight",
                Type = DataType.Number,
                Index = 6,
            });
        }

        if (!DataPointCategories.Any(x => x.Guid == new Guid("BF394F35-2228-4933-BF38-AF5B1B97AEF7")))
        {
            DataPointCategories.Add(new()
            {
                Guid = new Guid("BF394F35-2228-4933-BF38-AF5B1B97AEF7"),
                Group = "Notes",
                Type = DataType.Text,
            });
        }
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
            day = new()
            {
                Date = date,
            };

            await Days.AddAsync(day);
            await SaveChangesAsync();
        }

        // Add any missing data points for the main group of categories.
        var anyDataPointsAdded = false;
        foreach (var category in DataPointCategories.Where(c => c.Group == null))
        {
            if (!category.DataPoints.Where(x => x.Day == day).Any(x => x.Category.Guid == category.Guid))
            {
                category.DataPoints.Add(new()
                {
                    Day = day,
                    Category = category,
                    CreatedAt = DateTimeOffset.Now,
                    DataType = category.Type,
                });
                anyDataPointsAdded = true;
            }
        }

        if (anyDataPointsAdded)
            await SaveChangesAsync();

        return day;
    }

    public Task<Day> GetNextDay(Day day) => GetDay(day.Date.GetNextDate());

    public Task<Day> GetPreviousDay(Day day) => GetDay(day.Date.GetPreviousDate());
}