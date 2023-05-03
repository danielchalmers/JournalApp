namespace JournalApp;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
        Database.EnsureDeleted();
        Database.EnsureCreated();

        Days.Add(new Day()
        {
            Date = DateOnly.FromDateTime(DateTime.Now),
            DataPoints = new List<DataPoint>
            {
                new SleepDataPoint { Name = "Sleep" },
                new ScaleDataPoint { Name = "Happiness" },
                new BoolDataPoint { Name = "Productive?" },
                new NumberDataPoint { Name = "Weight" },
            },
            Notes = new List<NoteDataPoint>
            {
            }
        });

        SaveChanges();
    }

    public DbSet<Day> Days { get; set; } = default!;
}