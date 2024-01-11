namespace JournalApp;

public class AppDbSeeder(IDbContextFactory<AppDbContext> dbcf, ILogger<AppDbSeeder> _logger)
{
    public void SeedDb()
    {
        _logger.LogInformation("Seeding database");

        using var db = dbcf.CreateDbContext();
        var sw = Stopwatch.StartNew();
        try
        {
            db.Database.Migrate();
            db.SaveChanges();
        }
        catch (Microsoft.Data.Sqlite.SqliteException ex) when (ex.SqliteErrorCode == 14)
        {
            // https://stackoverflow.com/a/38562947.
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database migration error");
            throw;
        }

        sw.Stop();
        _logger.LogInformation($"Migrated database in {sw.ElapsedMilliseconds}ms");

        sw.Restart();
        SeedCategories();
        sw.Stop();
        _logger.LogInformation($"Seeded categories in {sw.ElapsedMilliseconds}ms");

#if DEBUG
        sw.Restart();
        SeedDays();
        sw.Stop();
        _logger.LogInformation($"Seeded days in {sw.ElapsedMilliseconds}ms");
#endif
    }

    private void SeedCategories()
    {
        using var db = dbcf.CreateDbContext();

        void AddOrUpdate(
            string guidString,
            PointType type,
            string group = null,
            string name = null,
            bool readOnly = false,
            bool enabled = true,
            decimal? medDose = null,
            string medUnit = null,
            DateTimeOffset? medEveryDaySince = null)
        {
            // Use existing category or create a new one.
            var guid = new Guid(guidString);
            var category = db.Categories.SingleOrDefault(x => x.Guid == guid);
            var doesExist = category != null;
            category ??= new();

            // Overwrite some properties that are always supposed to be static.
            category.Guid = guid;
            category.Type = type;
            category.Group = group;
            category.ReadOnly = readOnly;
            category.MedicationUnit = medUnit;

            // Overwrite some flexible properties if it doesn't already exist OR is readonly and isn't allowed to change.
            if (!doesExist || readOnly)
            {
                category.Name = name;
                category.Enabled = enabled;
                category.MedicationDose = medDose;
                category.MedicationEveryDaySince = medEveryDaySince;
                category.Deleted = false;
            }

            // Add the new category.
            if (!doesExist)
                db.AddCategory(category);

            // Save all the changed properties or the new category itself.
            db.SaveChanges();
        }

        AddOrUpdate(
            "BF394F35-2228-4933-BF38-AF5B1B97AEF7",
            PointType.Note,
            group: "Notes",
            readOnly: true
        );
        AddOrUpdate(
            "D90D89FB-F5B9-47CF-AE4E-3EC0D635E783",
            PointType.Mood,
            name: "Overall mood",
            readOnly: true
        );
        AddOrUpdate(
            "D8657B36-F3A0-486F-BF80-0CF057919C7D",
            PointType.Sleep,
            name: "Last night's sleep"
        );
        AddOrUpdate(
            "7330B995-0B56-46FF-9DD6-9CFC550FF5C8",
            PointType.MildToSevere,
            name: "Most depressed mood",
            enabled: false
        );
        AddOrUpdate(
            "4955EB49-0BCF-433B-873E-2092F292CC6B",
            PointType.MildToSevere,
            name: "Most elevated mood",
            enabled: false
        );
        AddOrUpdate(
            "E9B7E4BE-FD17-4171-B1D4-D38B6009FDA0",
            PointType.MildToSevere,
            name: "Irritability",
            enabled: false
        );
        AddOrUpdate(
            "0FB54AFF-9ECC-4C17-BAB5-B908B794CEA9",
            PointType.MildToSevere,
            name: "Anxiety",
            enabled: false
        );
        AddOrUpdate(
            "40B5AF7B-4F4E-4E77-BD6B-F7855CF773AB",
            PointType.LowToHigh,
            name: "Productivity"
        );
        AddOrUpdate(
            "DE394B38-9007-4349-AE31-429541AAB947",
            PointType.LowToHigh,
            name: "Physical activity"
        );
        AddOrUpdate(
            "EE8DE4D0-3A87-4CA4-B384-81BD7508A19F",
            PointType.Bool,
            name: "Menstruating",
            enabled: false
        );
        AddOrUpdate(
            "C871C9F7-1A6E-4EA2-ACC9-94A256C9E2CC",
            PointType.Bool,
            name: "Therapy",
            enabled: false
        );
        AddOrUpdate(
            "480DC07D-1330-486F-9B30-EC83A3D4E6F0",
            PointType.Number,
            name: "Weight"
        );
        AddOrUpdate(
            "2EEA42EE-4586-4E7D-ABF1-012BED1C0753",
            PointType.Medication,
            group: "Medications",
            name: "Example",
            enabled: true,
            medDose: 1,
            medUnit: " unit");
        AddOrUpdate(
            "01A8F325-3002-40C4-B076-234E26172E82",
            PointType.Medication,
            group: "Medications",
            name: "Vitamin D",
            enabled: false,
            medDose: 2000,
            medUnit: "IU",
            medEveryDaySince: DateTimeOffset.Now);
        AddOrUpdate(
            "7DBB09E6-4A83-4E71-94AE-C40F3422DF09",
            PointType.Medication,
            group: "Medications",
            name: "Cetirizine",
            enabled: false,
            medDose: 10,
            medUnit: "mg",
            medEveryDaySince: DateTimeOffset.Now);
        AddOrUpdate(
            "545E8EBE-3C5C-4289-B4AD-E11CAC7B9324",
            PointType.Medication,
            group: "Medications",
            name: "Trazadone",
            enabled: false,
            medDose: 50,
            medUnit: "mg",
            medEveryDaySince: DateTimeOffset.Now);
        AddOrUpdate(
            "A3AD777F-680A-481F-AFF2-E1CE6BCEA29E",
            PointType.Medication,
            group: "Medications",
            name: "Gabapentin",
            enabled: false,
            medDose: 300,
            medUnit: "mg",
            medEveryDaySince: DateTimeOffset.Now);
    }

    private void SeedDays(IEnumerable<DateOnly> dates)
    {
        using var db = dbcf.CreateDbContext();

        var categories = db.Categories.ToHashSet();
        var days = db.GetOrCreateDays(dates);

        var d = 0;
        while (true)
        {
            // Use the same seed over a random number of days to represent trends.
            var chunkLength = Random.Shared.Next(1, 6);
            var random = new Random(Guid.NewGuid().GetHashCode());

            for (var i = 0; i < chunkLength; i++)
            {
                // Sometimes don't even generate the day as if the user forgot.
                var fillDay = Random.Shared.Next(0, 10) > 0;

                // Get or create the day and fill in all missing points with the random seed.
                db.AddMissingPoints(days[d], categories, fillDay ? random : null);

                // Increment day and see if we're done.
                d++;
                if (d == days.Count)
                {
                    db.SaveChanges();
                    return;
                }
            }
        }
    }

    private void SeedDays()
    {
        var startDate = DateOnly.FromDateTime(DateTime.Now - TimeSpan.FromDays(120));
        var endDate = DateOnly.FromDateTime(DateTime.Now + TimeSpan.FromDays(7));
        var dates = new List<DateOnly>();

        dates.AddRange(startDate.DatesTo(endDate));

        // A few additional days to test multi-year features.
        foreach (var relativeMonth in new int[] { -12, -18, -24, -30, -36, -42, -48 })
            startDate.AddMonths(relativeMonth);

        SeedDays(dates);

        using var db = dbcf.CreateDbContext();

        db.SaveChanges();
    }
}
