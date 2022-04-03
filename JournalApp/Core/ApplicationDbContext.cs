using Microsoft.EntityFrameworkCore;

namespace JournalApp;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
        Database.EnsureDeleted();
        Database.EnsureCreated();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Note>().HasData(
            new Note()
            {
                Date = DateTime.Now - TimeSpan.FromDays(1),
                Text = "Yesterday's note"
            },
            new Note()
            {
                Date = DateTime.Now - TimeSpan.FromDays(7),
                Text = "Last week's note"
            },
            new Note()
            {
                Date = DateTime.Now - TimeSpan.FromDays(31),
                Text = "Last month's note"
            },
            new Note()
            {
                Date = DateTime.Now - TimeSpan.FromDays(1),
                Text = "Hello world, this is my note!"
            }
        );

        modelBuilder.Entity<JournalEntry>().HasData(
            new JournalEntry()
            {
                Date = DateTime.Now
            },
            new JournalEntry()
            {
                Date = DateTime.Now + TimeSpan.FromHours(1)
            }
        );
    }

    public DbSet<JournalEntry> Entries { get; set; } = default!;
}