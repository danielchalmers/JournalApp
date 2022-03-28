using Microsoft.EntityFrameworkCore;

namespace JournalApp;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
        //SQLitePCL.Batteries_V2.Init();
        Database.EnsureCreated();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Day>().HasData(
            new Day()
            {
                Date = DateOnly.FromDateTime(DateTime.Now),
                Text = "Hello, world!"
            }
        );

        base.OnModelCreating(modelBuilder);
    }

    public DbSet<Day> Days { get; set; }
}