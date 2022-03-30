using Microsoft.EntityFrameworkCore;

namespace JournalApp;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
        //SQLitePCL.Batteries_V2.Init();
        Database.EnsureDeleted();
        Database.EnsureCreated();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Note>().HasData(
            new Note()
            {
                Date = DateTime.Now,
                Text = "Hello world, this is my note!"
            }
        );
        modelBuilder.Entity<JournalEntry>().HasData(
            new JournalEntry()
            {
                Date = DateTime.Now
            }, new JournalEntry()
            {
                Date = DateTime.Now + TimeSpan.FromHours(1)
            }
        ); ;

        base.OnModelCreating(modelBuilder);
    }

    public DbSet<JournalEntry> Entries { get; set; }
}