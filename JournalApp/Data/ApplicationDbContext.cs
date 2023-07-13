namespace JournalApp;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
        try
        {
            Database.EnsureCreated();
        }
        catch (Microsoft.Data.Sqlite.SqliteException ex) when (ex.SqliteErrorCode == 14)
        {
            // https://stackoverflow.com/a/38562947.
        }

        SeedCategory(new()
        {
            Guid = new Guid("D90D89FB-F5B9-47CF-AE4E-3EC0D635E783"),
            Name = "Overall mood",
            Type = DataType.Mood,
            Index = 1,
        });
        SeedCategory(new()
        {
            Guid = new Guid("D8657B36-F3A0-486F-BF80-0CF057919C7D"),
            Name = "Last night's sleep",
            Type = DataType.Sleep,
            Index = 2,
        });
        SeedCategory(new()
        {
            Guid = new Guid("40B5AF7B-4F4E-4E77-BD6B-F7855CF773AB"),
            Name = "Productivity",
            Type = DataType.Scale,
            Index = 3,
        });
        SeedCategory(new()
        {
            Guid = new Guid("DE394B38-9007-4349-AE31-429541AAB947"),
            Name = "Exercised or was active",
            Type = DataType.Bool,
            Index = 4,
        });
        SeedCategory(new()
        {
            Guid = new Guid("480DC07D-1330-486F-9B30-EC83A3D4E6F0"),
            Name = "Weight",
            Type = DataType.Number,
            Index = 5,
        });
        SeedCategory(new()
        {
            Guid = new Guid("BF394F35-2228-4933-BF38-AF5B1B97AEF7"),
            Group = "Notes",
            Type = DataType.Note,
        });
        SeedCategory(new()
        {
            Guid = new Guid("01A8F325-3002-40C4-B076-234E26172E82"),
            Group = "Medications",
            Name = "Vitamin D",
            Type = DataType.Medication,
            Index = 0,
            MedicationDose = 2000,
            MedicationUnit = "IU",
            MedicationEveryDay = true,
            Enabled = false,
        });
    }

    private void SeedCategory(DataPointCategory category)
    {
        if (!Categories.Any(x => x.Guid == category.Guid))
        {
            Categories.Add(category);
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

    public DbSet<DataPointCategory> Categories { get; set; } = default!;

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

        if (AddMissingDataPoints(day))
            await SaveChangesAsync();

        return day;
    }

    public bool AddMissingDataPoints(Day day)
    {
        var anyDataPointsAdded = false;
        foreach (var category in Categories)
        {
            if (category.Group == "Notes")
            {
                if (category.DataPoints.Count == 0)
                {
                    var note = CreateNote(day);
                    note.Text = "I just started using JournalApp! 😎";
                    category.DataPoints.Add(note);
                }
            }
            else if (!category.DataPoints.Where(x => x.Day == day).Any(x => x.Category.Guid == category.Guid))
            {
                var dataPoint = new DataPoint()
                {
                    Day = day,
                    Category = category,
                    CreatedAt = DateTimeOffset.Now,
                    DataType = category.Type,
                    Dose = category.MedicationDose,
                    Unit = category.MedicationUnit,
                };

                if (category.MedicationEveryDay)
                    dataPoint.Bool = true;

                category.DataPoints.Add(dataPoint);
                anyDataPointsAdded = true;
            }
        }

        return anyDataPointsAdded;
    }

    public Task<Day> GetNextDay(Day day) => GetDay(day.Date.GetNextDate());

    public Task<Day> GetPreviousDay(Day day) => GetDay(day.Date.GetPreviousDate());

    public DataPoint CreateNote(Day day)
    {
        var notes = Categories.Single(x => x.Group == "Notes");

        return new()
        {
            Day = day,
            Category = notes,
            CreatedAt = DateTimeOffset.Now,
            DataType = DataType.Note,
        };
    }
}