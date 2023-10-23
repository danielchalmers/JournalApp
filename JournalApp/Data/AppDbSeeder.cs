namespace JournalApp;

public class AppDbSeeder
{
    private readonly AppDbContext db;
    private readonly ILogger<AppDbSeeder> _logger;

    public AppDbSeeder(AppDbContext dbContext, ILogger<AppDbSeeder> logger)
    {
        db = dbContext;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        var sw = Stopwatch.StartNew();
        bool? databaseWasCreated = null;
        try
        {
#if DEBUG
            //db.Database.EnsureDeleted();
#endif
            databaseWasCreated = db.Database.EnsureCreated();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ensure database error");

            if (ex is Microsoft.Data.Sqlite.SqliteException sqliteEx && sqliteEx.SqliteErrorCode == 14)
            {
                // https://stackoverflow.com/a/38562947.
            }
            else
            {
                throw;
            }
        }

        sw.Stop();
        _logger.LogInformation($"Ensured database was created in {sw.ElapsedMilliseconds}ms; Was created: {databaseWasCreated}");

        sw.Restart();
        await SeedCategories();
        sw.Stop();
        _logger.LogInformation($"Seeded categories in {sw.ElapsedMilliseconds}ms");

#if DEBUG
        sw.Restart();
        await SeedDays();
        sw.Stop();
        _logger.LogInformation($"Seeded days in {sw.ElapsedMilliseconds}ms");
#endif
    }

    private async Task SeedCategories()
    {
        async Task AddOrUpdate(
            string guid,
            DataType dataType,
            string group = null,
            string name = null,
            bool readOnly = true,
            bool enabled = true,
            decimal? medDose = null,
            string medUnit = null,
            DateTimeOffset? medEveryDaySince = null)
        {
            // Apply the given values to a new category, or an existing one without replacing it.

            var category = db.Categories.FirstOrDefault(x => x.Guid.ToString() == guid);
            var isNew = category == null;
            category ??= new();

            category.Group = group;
            category.Name = name;
            category.ReadOnly = readOnly;
            category.Enabled = enabled;
            category.Type = dataType;
            category.MedicationDose = medDose;
            category.MedicationUnit = medUnit;
            category.MedicationEveryDaySince = medEveryDaySince;

            if (isNew)
                await db.AddCategory(category);
        }

        await AddOrUpdate(
            "BF394F35-2228-4933-BF38-AF5B1B97AEF7",
            DataType.Note,
            group: "Notes"
        );
        await AddOrUpdate(
            "D90D89FB-F5B9-47CF-AE4E-3EC0D635E783",
            DataType.Mood,
            name: "Overall mood"
        );
        await AddOrUpdate(
            "D8657B36-F3A0-486F-BF80-0CF057919C7D",
            DataType.Sleep,
            name: "Last night's sleep"
        );
        await AddOrUpdate(
            "7330B995-0B56-46FF-9DD6-9CFC550FF5C8",
            DataType.MildToSevere,
            name: "Most depressed mood",
            enabled: false
        );
        await AddOrUpdate(
            "4955EB49-0BCF-433B-873E-2092F292CC6B",
            DataType.MildToSevere,
            name: "Most elevated mood",
            enabled: false
        );
        await AddOrUpdate(
            "E9B7E4BE-FD17-4171-B1D4-D38B6009FDA0",
            DataType.MildToSevere,
            name: "Irritability",
            enabled: false
        );
        await AddOrUpdate(
            "0FB54AFF-9ECC-4C17-BAB5-B908B794CEA9",
            DataType.MildToSevere,
            name: "Anxiety",
            enabled: false
        );
        await AddOrUpdate(
            "40B5AF7B-4F4E-4E77-BD6B-F7855CF773AB",
            DataType.LowToHigh,
            name: "Productivity"
        );
        await AddOrUpdate(
            "DE394B38-9007-4349-AE31-429541AAB947",
            DataType.Bool,
            name: "Exercised or was active"
        );
        await AddOrUpdate(
            "EE8DE4D0-3A87-4CA4-B384-81BD7508A19F",
            DataType.Bool,
            name: "Menstruating",
            enabled: false
        );
        await AddOrUpdate(
            "C871C9F7-1A6E-4EA2-ACC9-94A256C9E2CC",
            DataType.Bool,
            name: "Therapy",
            enabled: false
        );
        await AddOrUpdate(
            "480DC07D-1330-486F-9B30-EC83A3D4E6F0",
            DataType.Number,
            name: "Weight"
        );
        await AddOrUpdate(
            "01A8F325-3002-40C4-B076-234E26172E82",
            DataType.Medication,
            group: "Medications",
            name: "Vitamin D",
            medDose: 2000,
            medUnit: "IU",
            medEveryDaySince: DateTimeOffset.Now,
            readOnly: false,
            enabled: false
        );
    }

    private async Task SeedDays()
    {
        var startDate = DateOnly.FromDateTime(DateTime.Now - TimeSpan.FromDays(120));
        var endDate = DateOnly.FromDateTime(DateTime.Now + TimeSpan.FromDays(7));

        foreach (var dates in startDate.DatesTo(endDate).Chunk(3))
        {
            // We want the same values over a batch of days to represent trends.
            var seed = Guid.NewGuid().GetHashCode();
            foreach (var date in dates)
            {
                // Sometimes don't even generate the day as if the user forgot.
                var fillDay = Random.Shared.Next(0, 10) > 0;

                await db.GetOrCreateDay(date, false, fillDay ? new(seed) : null);
            }
        }

        // A few additional days to test multi-year features.
        foreach (var relativeMonth in new int[] { -12, -18, -24, -30, -36, -42, -48 })
        {
            await db.GetOrCreateDay(startDate.AddMonths(relativeMonth), false, Random.Shared);
        }

        await db.SaveChangesAsync();
    }
}
